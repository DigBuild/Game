using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DigBuild.Platform.Resource;

namespace DigBuild
{
    public sealed class ShaderCompiler : IResourceProvider
    {
        private readonly string _outputDir;

        public ShaderCompiler(string outputDir)
        {
            _outputDir = outputDir;
        }

        public IReadOnlySet<ResourceName> GetAndClearModifiedResources(GetAndClearModifiedResourcesDelegate parent)
        {
            return parent();
        }

        public IResource? GetResource(ResourceName name, GetResourceDelegate parent)
        {
            var resource = parent(name);
            if (resource != null || !name.Path.ToLower().EndsWith(".spv"))
                return resource;
            
            var originalPath = name.Path[..^4];
            var originalResource = parent(new ResourceName(name.Domain, originalPath));
            if (originalResource == null)
                return null;

            var fullPath = Path.GetFullPath(
                Path.TrimEndingDirectorySeparator(_outputDir) +
                Path.DirectorySeparatorChar +
                name.Domain +
                Path.DirectorySeparatorChar +
                name.Path
            );
            var lastWrite = File.GetLastWriteTime(fullPath);

            var shaderType = originalPath[(originalPath.LastIndexOf('.') + 1)..];

            if (!File.Exists(fullPath) || lastWrite < originalResource.LastEdited)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

                Process process = new()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        WindowStyle = ProcessWindowStyle.Hidden,
                        FileName = "glslc",
                        Arguments = $"-fshader-stage={shaderType} - -o \"{fullPath}\"",
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true
                    }
                };
                process.Start();
            
                originalResource.OpenStream().CopyTo(process.StandardInput.BaseStream);
                process.StandardInput.Flush();
                process.StandardInput.Close();
                process.WaitForExit();
            }
            return new Resource(fullPath, name, lastWrite);
        }

        private sealed class Resource : ResourceBase
        {
            private readonly string _path;
            public override ResourceName Name { get; }
            public override DateTime LastEdited { get; }

            public Resource(string path, ResourceName name, DateTime lastEdited)
            {
                _path = path;
                Name = name;
                LastEdited = lastEdited;
            }

            public override Stream OpenStream()
            {
                return File.OpenRead(_path);
            }
        }
    }
}