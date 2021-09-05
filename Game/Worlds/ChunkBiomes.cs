using System;
using System.IO;
using DigBuild.Engine.Math;
using DigBuild.Engine.Serialization;
using DigBuild.Engine.Storage;
using DigBuild.Engine.Worlds;
using DigBuild.Platform.Resource;
using DigBuild.Registries;
using DigBuild.Worldgen.Biomes;

namespace DigBuild.Worlds
{
    /// <summary>
    /// A read-only representation of a chunk's biomes.
    /// </summary>
    public interface IReadOnlyChunkBiomes : IChangeNotifier
    {
        /// <summary>
        /// Gets the biome at a given position.
        /// </summary>
        /// <param name="x">The X coordinate</param>
        /// <param name="z">The Z coordinate</param>
        /// <returns>The biome</returns>
        IBiome Get(int x, int z);
    }

    /// <summary>
    /// A chunk-level biome storage.
    /// </summary>
    public sealed class ChunkBiomes : IReadOnlyChunkBiomes, IData<ChunkBiomes>
    {
        private const uint ChunkSize = WorldDimensions.ChunkWidth;

        private readonly IBiome[,] _biomes = new IBiome[ChunkSize, ChunkSize];

        public event Action? Changed;

        /// <summary>
        /// Sets the biome at a given position.
        /// </summary>
        /// <param name="x">The X coordinate</param>
        /// <param name="z">The Z coordinate</param>
        /// <param name="biome">The biome</param>
        public void Set(int x, int z, IBiome biome)
        {
            _biomes[x, z] = biome;
            Changed?.Invoke();
        }

        public IBiome Get(int x, int z)
        {
            return _biomes[x & 15, z & 15];
        }
        
        public ChunkBiomes Copy()
        {
            var storage = new ChunkBiomes();

            for (var x = 0; x < ChunkSize; x++)
            for (var z = 0; z < ChunkSize; z++)
                storage._biomes[x, z] = _biomes[x, z];

            return storage;
        }

        /// <summary>
        /// The serdes.
        /// </summary>
        public static ISerdes<ChunkBiomes> Serdes { get; } = new SimpleSerdes<ChunkBiomes>(
            (stream, biomes) =>
            {
                var bw = new BinaryWriter(stream);

                for (var x = 0; x < ChunkSize; x++)
                for (var z = 0; z < ChunkSize; z++)
                {
                    var biome = biomes._biomes[x, z];
                    var registryName = GameRegistries.Biomes.GetNameOrNull(biome)!.Value;
                    bw.Write(registryName.ToString());
                }
            },
            (stream, _) =>
            {
                var br = new BinaryReader(stream);
                var biomes = new ChunkBiomes();
                
                for (var x = 0; x < ChunkSize; x++)
                for (var z = 0; z < ChunkSize; z++)
                {
                    var registryName = ResourceName.Parse(br.ReadString())!.Value;
                    var biome = GameRegistries.Biomes.GetOrNull(registryName)!;
                    biomes._biomes[x, z] = biome;
                }

                return biomes;
            }
        );
    }

    /// <summary>
    /// Helpers for chunk biome access.
    /// </summary>
    public static class ChunkBiomeExtensions
    {
        /// <summary>
        /// Gets the biome at a certain position in the world.
        /// </summary>
        /// <param name="world">The world</param>
        /// <param name="pos">The position</param>
        /// <returns>The biome</returns>
        public static IBiome GetBiome(this IReadOnlyWorld world, BlockPos pos)
        {
            var (x, _, z) = pos.SubChunkPos;
            return world.GetChunk(pos.ChunkPos)!.Get(GameChunkStorages.Biomes).Get(x, z);
        }
    }
}