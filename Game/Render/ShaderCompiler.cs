using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DigBuild.Platform.Resource;

namespace DigBuild.Render
{
    /// <summary>
    /// A resource compiler that invokes <c>shaderc</c> to compile shaders automatically at runtime.
    /// </summary>
    public sealed class ShaderCompiler : IResourceProvider
    {
        private readonly string _outputDir;
        private readonly HashSet<ResourceName> _uncompiledShaders = new();

        public ShaderCompiler(string outputDir)
        {
            _outputDir = outputDir;
        }

        public void AddAndClearModifiedResources(ISet<ResourceName> resources)
        {
            foreach (var resource in _uncompiledShaders.Where(resources.Contains))
                resources.Add(new ResourceName(resource.Domain, resource.Path + ".spv"));
        }

        public IResource? GetResource(ResourceName name, GetResourceDelegate parent)
        {
            var resource = parent(name);
            if (resource != null || !name.Path.ToLower().EndsWith(".spv"))
            {
                _uncompiledShaders.Remove(new ResourceName(name.Domain, name.Path[..^4]));
                return resource;
            }
            
            var originalPath = name.Path[..^4];
            var originalResourceName = new ResourceName(name.Domain, originalPath);
            var originalResource = parent(originalResourceName);
            if (originalResource == null)
                return null;

            _uncompiledShaders.Add(originalResourceName);

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

                var srcPath = originalResource.FileSystemPath;

                Process process = new()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        WindowStyle = ProcessWindowStyle.Hidden,
                        FileName = "glslc",
                        Arguments = $"-fshader-stage={shaderType} - -o \"{fullPath}\"",
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        WorkingDirectory = (srcPath != null ? Path.GetDirectoryName(srcPath) : null)!
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