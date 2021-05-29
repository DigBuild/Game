using System;
using System.Collections.Immutable;
using DigBuild.Content.Registries;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Collections;
using DigBuild.Engine.Impl.Worlds;
using DigBuild.Engine.Math;
using DigBuild.Engine.Worldgen;
using DigBuild.Engine.Worlds;
using IWorldgenFeature = DigBuild.Engine.Worldgen.IWorldgenFeature;

namespace DigBuild.Content.Worldgen
{
    public class TerrainWorldgenFeature : IWorldgenFeature
    {
        private const uint ChunkSize = 16;

        private readonly FastNoiseLite _terrainHeightNoise = new();

        private readonly Block _terrainBlock, _surfaceBlock;

        public IImmutableSet<IWorldgenAttribute> InputAttributes => ImmutableHashSet.Create<IWorldgenAttribute>();

        public IImmutableSet<IWorldgenAttribute> OutputAttributes => ImmutableHashSet.Create<IWorldgenAttribute>(
            WorldgenAttributes.TerrainHeight, WorldgenAttributes.TerrainType
        );

        public TerrainWorldgenFeature(Block terrainBlock, Block surfaceBlock)
        {
            _terrainBlock = terrainBlock;
            _surfaceBlock = surfaceBlock;

            _terrainHeightNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            _terrainHeightNoise.SetFrequency(0.001f);
            _terrainHeightNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
            _terrainHeightNoise.SetFractalOctaves(4);
            _terrainHeightNoise.SetFractalLacunarity(2.4f);
            _terrainHeightNoise.SetFractalGain(0.4f);
        }

        public void DescribeSlice(WorldSliceDescriptionContext context)
        {
            var height = Grid<ushort>.Builder(ChunkSize);
            var terrainType = Grid<TerrainType>.Builder(ChunkSize, TerrainType.Ground);

            var inBiome = context.Get(WorldgenAttributes.Biome);

            _terrainHeightNoise.SetSeed((int) context.Seed);
            for (var x = 0; x < ChunkSize; x++)
            {
                for (var z = 0; z < ChunkSize; z++)
                {
                    var noise = _terrainHeightNoise.GetNoise(context.Position.X * ChunkSize + x, context.Position.Z * ChunkSize + z) * 0.5f + 0.5f;
                    var biome = inBiome[x, z];

                    var heightRange = biome.GetConstraints(WorldgenAttributes.TerrainHeight) ?? new RangeT<ushort>(2, 30);
                    var minHeight = heightRange.Start;
                    var heightDelta = heightRange.End - heightRange.Start;

                    height[x, z] = (ushort) (minHeight + heightDelta * noise);
                }
            }

            context.Submit(WorldgenAttributes.TerrainHeight, height.Build());
            context.Submit(WorldgenAttributes.TerrainType, terrainType.Build());
        }

        public void PopulateChunk(WorldSliceDescriptor descriptor, IChunk chunk)
        {
            var height = descriptor.Get(WorldgenAttributes.TerrainHeight);
            var inBiome = descriptor.Get(WorldgenAttributes.Biome);
            for (var x = 0; x < ChunkSize; x++)
            {
                for (var z = 0; z < ChunkSize; z++)
                {
                    var relativeHeight = height[x, z] - chunk.Position.Y * ChunkSize;
                    if (relativeHeight <= 0)
                        continue;
                    var localHeight = Math.Min(relativeHeight, ChunkSize);
                    var biome = inBiome[x, z];
                    for (var y = 0; y < localHeight - 1; y++)
                        chunk.SetBlock(new ChunkBlockPos(x, y, z), _terrainBlock);
                    
                    chunk.SetBlock(new ChunkBlockPos(x, (int) (localHeight - 1), z), localHeight == relativeHeight ? biome.SurfaceBlock : _terrainBlock);
                }
            }
        }
    }

    public enum TerrainType
    {
        Ground,
        Water,
        // Structure
    }
}