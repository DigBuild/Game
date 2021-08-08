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
    public interface IReadOnlyChunkBiomes : IChangeNotifier
    {
        IBiome Get(int x, int z);

        IBiome Get(ChunkBlockPos pos);
    }

    public sealed class ChunkBiomes : IReadOnlyChunkBiomes, IData<ChunkBiomes>
    {
        private const uint ChunkSize = WorldDimensions.ChunkSize;

        private readonly IBiome[,] _biomes = new IBiome[ChunkSize, ChunkSize];

        public event Action? Changed;

        public void Set(int x, int z, IBiome biome)
        {
            _biomes[x, z] = biome;
            Changed?.Invoke();
        }

        public IBiome Get(int x, int z)
        {
            return _biomes[x & 15, z & 15];
        }

        public IBiome Get(ChunkBlockPos pos) => Get(pos.X, pos.Z);

        public ChunkBiomes Copy()
        {
            var storage = new ChunkBiomes();

            for (var x = 0; x < ChunkSize; x++)
            for (var z = 0; z < ChunkSize; z++)
                storage._biomes[x, z] = _biomes[x, z];

            return storage;
        }

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

    public static class ChunkBiomeExtensions
    {
        public static IBiome GetBiome(this IReadOnlyWorld world, BlockPos pos)
        {
            return world.GetChunk(pos.ChunkPos)!.Get(GameChunkStorages.Biomes).Get(pos.SubChunkPos);
        }
    }
}