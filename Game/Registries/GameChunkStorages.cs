using DigBuild.Engine.Registries;
using DigBuild.Engine.Storage;
using DigBuild.Engine.Worlds;
using DigBuild.Engine.Worlds.Impl;
using DigBuild.Platform.Resource;
using DigBuild.Worlds;

namespace DigBuild.Registries
{
    /// <summary>
    /// The game's chunk-local data storage.
    /// </summary>
    public static class GameChunkStorages
    {
        /// <summary>
        /// Biome storage.
        /// </summary>
        public static DataHandle<IChunk, IReadOnlyChunkBiomes, ChunkBiomes> Biomes { get; private set; } = null!;

        internal static void Register(RegistryBuilder<IDataHandle<IChunk>> registry)
        {
            IChunkBlockLight.Type = registry.Register<IChunk, IReadOnlyChunkBlockLight, IChunkBlockLight>(
                new ResourceName(DigBuildGame.Domain, "block_light"),
                () => new ChunkBlockLight(), ChunkBlockLight.Serdes
            );
            
            Biomes = registry.Register<IChunk, IReadOnlyChunkBiomes, ChunkBiomes>(
                new ResourceName(DigBuildGame.Domain, "biomes"),
                () => new ChunkBiomes(), ChunkBiomes.Serdes
            );
        }
    }
}