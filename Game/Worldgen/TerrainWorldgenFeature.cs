using System;
using System.Collections.Immutable;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Math;
using DigBuild.Engine.Worldgen;
using DigBuild.Engine.Worlds;
using DigBuild.Voxel;
using IWorldgenFeature = DigBuild.Engine.Worldgen.IWorldgenFeature;

namespace DigBuild.Worldgen
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
            _terrainHeightNoise.SetFractalOctaves(2);
            _terrainHeightNoise.SetFractalLacunarity(2.4f);
            _terrainHeightNoise.SetFractalGain(0.4f);
        }

        public void DescribeSlice(WorldSliceDescriptionContext context)
        {
            var height = new ImmutableMap2DBuilder<ushort>(ChunkSize);
            var terrainType = new ImmutableMap2DBuilder<TerrainType>(ChunkSize, TerrainType.Ground);

            _terrainHeightNoise.SetSeed((int) context.Seed);
            for (int x = 0; x < ChunkSize; x++)
            {
                for (int z = 0; z < ChunkSize; z++)
                {
                    var noise = _terrainHeightNoise.GetNoise(context.Position.X * ChunkSize + x, context.Position.Z * ChunkSize + z) * 0.5f + 0.5f;
                    height[x, z] = (ushort) (2 + 30 * noise);
                }
            }

            context.Submit(WorldgenAttributes.TerrainHeight, height.Build());
            context.Submit(WorldgenAttributes.TerrainType, terrainType.Build());
        }

        public void PopulateChunk(WorldSliceDescriptor descriptor, IChunk chunk)
        {
            var height = descriptor.Get(WorldgenAttributes.TerrainHeight);
            for (int x = 0; x < ChunkSize; x++)
            {
                for (int z = 0; z < ChunkSize; z++)
                {
                    var relativeHeight = height[x, z] - chunk.Position.Y * ChunkSize;
                    if (relativeHeight <= 0)
                        continue;
                    var localHeight = Math.Min(relativeHeight, ChunkSize);
                    for (int y = 0; y < localHeight - 1; y++)
                        chunk.SetBlock(new BlockPos(x, y, z), _terrainBlock);
                    
                    chunk.SetBlock(new BlockPos(x, (int) (localHeight - 1), z), localHeight == relativeHeight ? _surfaceBlock : _terrainBlock);
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