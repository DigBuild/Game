using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using DigBuild.Engine.Worldgen;
using DigBuild.Registries;
using DigBuild.Serialization;

namespace DigBuild
{
    /// <summary>
    /// A game configuration.
    /// </summary>
    public sealed class Config
    {
        [JsonIgnore]
        private string _path = null!;

        /// <summary>
        /// The world generation seed.
        /// </summary>
        public long Seed { get; set; } = RandomLong();

        /// <summary>
        /// The world generation options.
        /// </summary>
        public WorldgenT Worldgen { get; set; } = new();

        /// <summary>
        /// A set of world generation options.
        /// </summary>
        public sealed class WorldgenT
        {
            /// <summary>
            /// The active features.
            /// </summary>
            public List<IWorldgenFeature> Features { get; set; } = new()
            {
                // WorldgenFeatures.Terrain,
                // WorldgenFeatures.Water,
                // WorldgenFeatures.Lushness,
                // WorldgenFeatures.Trees
            };
        }

        /// <summary>
        /// Saves the config to disk.
        /// </summary>
        public void Save()
        {
            var fileContents = JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                PropertyNamingPolicy = new JsonSnakeCaseNamingPolicy(),
                DictionaryKeyPolicy = new JsonSnakeCaseNamingPolicy(),
                WriteIndented = true,
                Converters =
                {
                    new JsonStringRegistryEntryConverter<IWorldgenFeature>(GameRegistries.WorldgenFeatures)
                }
            });
            var parentDir = Path.GetDirectoryName(_path) ?? string.Empty;
            if (parentDir.Length > 0)
                Directory.CreateDirectory(parentDir);
            File.WriteAllText(_path, fileContents);
        }

        /// <summary>
        /// Loads a config from disk or creates a new one.
        /// </summary>
        /// <param name="path">The path to the config</param>
        /// <returns>The config</returns>
        public static Config Load(string path)
        {
            var cfg = default(Config);
            
            var fileContents = File.Exists(path) ? File.ReadAllText(path) : null;
            if (fileContents != null)
            {
                try
                {
                    cfg = JsonSerializer.Deserialize<Config>(fileContents, new JsonSerializerOptions
                    {
                        AllowTrailingCommas = true,
                        ReadCommentHandling = JsonCommentHandling.Skip,
                        PropertyNamingPolicy = new JsonSnakeCaseNamingPolicy(),
                        DictionaryKeyPolicy = new JsonSnakeCaseNamingPolicy(),
                        Converters =
                        {
                            new JsonStringRegistryEntryConverter<IWorldgenFeature>(GameRegistries.WorldgenFeatures)
                        }
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception thrown while loading server config: {ex.Message}");
                }
            }

            cfg ??= new Config();
            cfg._path = path;
            
            cfg.Save();

            return cfg;
        }

        private static long RandomLong()
        {
            var rnd = new Random();
            return ((long)rnd.Next() << 32) | (uint)rnd.Next();
        }
    }
}