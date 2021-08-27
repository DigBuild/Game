using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DigBuild.Engine.Events;

namespace DigBuild.Modding
{
    public class ModLoader
    {
        public static ModLoader Instance { get; } = new();

        private readonly List<ModContainer> _mods = new();
        private bool _loaded;

        public IEnumerable<ModContainer> Mods => _mods;

        public void LoadMods(EventBus eventBus)
        {
            if (_loaded)
                throw new Exception("Mods are already loaded.");
            _loaded = true;
            
            // Content assembly
            _mods.Add(new ModContainer(Assembly.LoadFile(Path.GetFullPath("DigBuild.Content.dll"))));

            // Mods
            if (Directory.Exists("mods"))
            {
                var candidates = Directory.EnumerateFiles("mods");
                foreach (var candidate in candidates)
                {
                    if (!candidate.ToLowerInvariant().EndsWith(".dll"))
                        continue;
                    try
                    {
                        _mods.Add(new ModContainer(Assembly.LoadFile(Path.GetFullPath(candidate))));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }

            foreach (var mod in _mods) 
                mod.Instance.Setup(eventBus); 
        }
    }
}