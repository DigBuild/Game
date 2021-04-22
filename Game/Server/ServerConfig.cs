using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using DigBuild.Engine.Worldgen;
using DigBuild.Registries;
using DigBuild.Serialization;

namespace DigBuild.Server
{
    public sealed class ServerConfig
    {
        [JsonIgnore]
        private string _path = null!;

        public ushort Port { get; set; } = 1234;
        public WorldgenT Worldgen { get; set; } = new();
        
        public sealed class WorldgenT
        {
            public List<IWorldgenFeature> Features { get; set; } = new()
            {
                // WorldgenFeatures.Terrain,
                // WorldgenFeatures.Water,
                // WorldgenFeatures.Lushness,
                // WorldgenFeatures.Trees
            };
        }

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
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            File.WriteAllText(_path, fileContents);
        }

        public static ServerConfig Load(string path)
        {
            var cfg = default(ServerConfig);
            
            var fileContents = File.Exists(path) ? File.ReadAllText(path) : null;
            if (fileContents != null)
            {
                try
                {
                    cfg = JsonSerializer.Deserialize<ServerConfig>(fileContents, new JsonSerializerOptions
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

            cfg ??= new ServerConfig();
            cfg._path = path;
            
            cfg.Save();

            return cfg;
        }
    }
}