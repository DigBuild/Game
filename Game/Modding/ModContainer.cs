using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DigBuild.Platform.Resource;

namespace DigBuild.Modding
{
    /// <summary>
    /// A mod container.
    /// </summary>
    public sealed class ModContainer
    {
        /// <summary>
        /// The assembly containing the mod.
        /// </summary>
        public Assembly Assembly { get; }
        
        /// <summary>
        /// The mod's domain.
        /// </summary>
        public string Domain { get; }
        /// <summary>
        /// The mod's instance.
        /// </summary>
        public IMod Instance { get; }

        /// <summary>
        /// The mod's resource provider.
        /// </summary>
        public IResourceProvider Resources { get; }

        private ModContainer(Assembly assembly)
        {
            Assembly = assembly;

            var resourcePrefix = assembly.GetName().Name + ".Resources";
            
            var modFileStream = assembly.GetManifestResourceStream($"{resourcePrefix}.digbuild-mod.txt");
            if (modFileStream == null)
                throw new Exception($"Could not find digbuild-mod.txt in {assembly.FullName}");
            var modFileReader = new StreamReader(modFileStream);
            var modFile = modFileReader.ReadToEnd();

            var mainPath = modFile.Trim();
            Instance = (IMod)assembly.CreateInstance(mainPath)!;
            Domain = Instance.Domain;

            Resources = new ResourceProvider(assembly, resourcePrefix, Domain);
        }

        private sealed class ResourceProvider : IResourceProvider
        {
            private readonly Assembly _assembly;
            private readonly string _resourcePrefix;
            private readonly string _domain;

            public ResourceProvider(Assembly assembly, string resourcePrefix, string domain)
            {
                _assembly = assembly;
                _resourcePrefix = resourcePrefix;
                _domain = domain;
            }

            public void AddAndClearModifiedResources(ISet<ResourceName> resources) { }

            public IResource? GetResource(ResourceName name, GetResourceDelegate parent)
            {
                if (name.Domain != _domain)
                    return parent(name);

                var formattedPath = name.Path.Replace('/', '.');
                var fullPath = $"{_resourcePrefix}.{formattedPath}";
                var stream = _assembly.GetManifestResourceStream(fullPath);

                return stream != null ? new Resource(name, _assembly, fullPath, stream) : parent(name);
            }

            private sealed class Resource : ResourceBase
            {
                private readonly Assembly _assembly;
                private readonly string _path;
                private Stream? _stream;
                
                public override ResourceName Name { get; }
                public override DateTime LastEdited { get; }

                public Resource(ResourceName name, Assembly assembly, string path, Stream stream)
                {
                    Name = name;
                    LastEdited = DateTime.MinValue;
                    _assembly = assembly;
                    _path = path;
                    _stream = stream;
                }

                public override Stream OpenStream()
                {
                    if (_stream == null)
                        return _assembly.GetManifestResourceStream(_path)!;

                    var stream = _stream;
                    _stream = null;
                    return stream;

                }
            }
        }

        /// <summary>
        /// Creates a new mod container from a file path.
        /// </summary>
        /// <param name="path">The path</param>
        /// <returns>The mod container</returns>
        public static ModContainer FromFile(string path)
        {
            return new ModContainer(Assembly.LoadFile(Path.GetFullPath(path)));
        }
    }
}