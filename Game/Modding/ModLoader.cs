using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace DigBuild.Modding
{
    public class ModLoader
    {
        public static ModLoader Instance { get; } = new ModLoader();

        private readonly List<IMod> _mods = new();
        private bool _loaded;

        public IEnumerable<IMod> Mods => _mods;

        public void LoadMods()
        {
            if (_loaded)
                throw new Exception("Mods are already loaded.");
            _loaded = true;

            var content = Assembly.LoadFile(Path.GetFullPath("DigBuild.Content.dll"));
            _mods.Add((IMod) content.CreateInstance("DigBuild.Content.ContentMod")!);
        }
    }
}