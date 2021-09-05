using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DigBuild.Engine.Events;

namespace DigBuild.Modding
{
    /// <summary>
    /// A mod loader and manager.
    /// </summary>
    public sealed class ModLoader
    {
        /// <summary>
        /// The instance.
        /// </summary>
        public static ModLoader Instance { get; } = new();

        private readonly List<ModContainer> _mods = new();
        private bool _loaded;

        /// <summary>
        /// The currently loaded mods.
        /// </summary>
        public IEnumerable<ModContainer> Mods => _mods;

        /// <summary>
        /// Looks for and loads all the mods.
        /// </summary>
        /// <param name="eventBus">The event bus</param>
        public void LoadMods(EventBus eventBus)
        {
            if (_loaded)
                throw new Exception("Mods are already loaded.");
            _loaded = true;
            
            // Content assembly
            _mods.Add(ModContainer.FromFile("DigBuild.Content.dll"));

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
                        _mods.Add(ModContainer.FromFile(candidate));
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