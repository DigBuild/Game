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

        private readonly List<IMod> _mods = new();
        private bool _loaded;

        public IEnumerable<IMod> Mods => _mods;

        public void LoadMods(EventBus eventBus)
        {
            if (_loaded)
                throw new Exception("Mods are already loaded.");
            _loaded = true;

            var content = Assembly.LoadFile(Path.GetFullPath("DigBuild.Content.dll"));
            _mods.Add((IMod) content.CreateInstance("DigBuild.Content.ContentMod")!);
            
            foreach (var mod in _mods) 
                mod.Setup(eventBus); 
        }
    }
}