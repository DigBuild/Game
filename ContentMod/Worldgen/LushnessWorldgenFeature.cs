using System.Collections.Immutable;
using DigBuild.Content.Registries;
using DigBuild.Engine.Collections;
using DigBuild.Engine.Math;
using DigBuild.Engine.Worldgen;
using DigBuild.Engine.Worlds;

namespace DigBuild.Content.Worldgen
{
    public sealed class LushnessWorldgenFeature : IWorldgenFeature
    {
        private const uint ChunkSize = 16;

        private readonly FastNoiseLite _lushnessNoise = new();

        public IImmutableSet<IWorldgenAttribute> InputAttributes => ImmutableHashSet.Create<IWorldgenAttribute>(
            WorldgenAttributes.TerrainType
        );

        public IImmutableSet<IWorldgenAttribute> OutputAttributes => ImmutableHashSet.Create<IWorldgenAttribute>(
            WorldgenAttributes.Lushness
        );

        public LushnessWorldgenFeature()
        {
            _lushnessNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            _lushnessNoise.SetFrequency(0.005f);
            _lushnessNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
            _lushnessNoise.SetFractalOctaves(2);
            _lushnessNoise.SetFractalLacunarity(2.0f);
            _lushnessNoise.SetFractalGain(0.5f);
        }

        public void DescribeSlice(WorldSliceDescriptionContext context)
        {
            var lushness = Grid<byte>.Builder(ChunkSize);
            var terrainTypeIn = context.Get(WorldgenAttributes.TerrainType);
            
            _lushnessNoise.SetSeed((int) context.Seed);
            for (var x = 0; x < ChunkSize; x++)
            for (var z = 0; z < ChunkSize; z++)
            {
                if (terrainTypeIn[x, z] != TerrainType.Ground)
                    continue;

                var (nx, nz) = (context.Position.X * ChunkSize + x, context.Position.Z * ChunkSize + z);
                lushness[x, z] = (byte) ((_lushnessNoise.GetNoise(nx, nz) + 1) * 0.5 * 255);
            }

            context.Submit(WorldgenAttributes.Lushness, lushness.Build());
        }

        public void PopulateChunk(WorldSliceDescriptor descriptor, IChunk chunk)
        {
        }
    }
}