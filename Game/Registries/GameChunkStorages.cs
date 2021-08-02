using DigBuild.Engine.Registries;
using DigBuild.Engine.Storage;
using DigBuild.Engine.Worlds;
using DigBuild.Engine.Worlds.Impl;
using DigBuild.Platform.Resource;
using DigBuild.Worlds;

namespace DigBuild.Registries
{
    public static class GameChunkStorages
    {
        public static DataHandle<IChunk, IReadOnlyChunkBiomes, ChunkBiomes> Biomes { get; private set; } = null!;

        public static void Register(RegistryBuilder<IDataHandle<IChunk>> registry)
        {
            IChunkBlockLight.Type = registry.Create<IChunk, IReadOnlyChunkBlockLight, IChunkBlockLight>(
                new ResourceName(DigBuildGame.Domain, "block_light"),
                () => new ChunkBlockLight(), ChunkBlockLight.Serdes
            );
            
            Biomes = registry.Create<IChunk, IReadOnlyChunkBiomes, ChunkBiomes>(
                new ResourceName(DigBuildGame.Domain, "biomes"),
                () => new ChunkBiomes(), ChunkBiomes.Serdes
            );
        }
    }
}