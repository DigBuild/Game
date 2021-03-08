using System;
using System.Collections.Immutable;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Math;
using DigBuild.Engine.Voxel;
using DigBuild.Engine.Worldgen;
using IWorldgenAttribute = DigBuild.Engine.Worldgen.IWorldgenAttribute;
using IWorldgenFeature = DigBuild.Engine.Worldgen.IWorldgenFeature;

namespace DigBuild.Worldgen
{
    public class WaterWorldgenFeature : IWorldgenFeature
    {
        private const uint ChunkSize = 16;
        private const float Threshold = -0.6f;

        private readonly FastNoiseLite _waterNoise = new();

        private readonly Block _waterBlock;

        public IImmutableSet<IWorldgenAttribute> InputAttributes => ImmutableHashSet.Create<IWorldgenAttribute>(
            WorldgenAttributes.TerrainHeight,
            WorldgenAttributes.TerrainType
        );

        public IImmutableSet<IWorldgenAttribute> OutputAttributes => ImmutableHashSet.Create<IWorldgenAttribute>(
            // WorldgenAttributes.TerrainHeight,
            WorldgenAttributes.TerrainType,
            WorldgenAttributes.WaterHeight
        );

        public WaterWorldgenFeature(Block waterBlock)
        {
            _waterBlock = waterBlock;

            _waterNoise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
            _waterNoise.SetFrequency(0.015f);
            _waterNoise.SetFractalType(FastNoiseLite.FractalType.None);
            _waterNoise.SetCellularDistanceFunction(FastNoiseLite.CellularDistanceFunction.EuclideanSq);
            _waterNoise.SetCellularReturnType(FastNoiseLite.CellularReturnType.Distance2Mul);
            _waterNoise.SetCellularJitter(1.0f);
            // _waterNoise.SetDomainWarpType(FastNoiseLite.DomainWarpType.OpenSimplex2Reduced);
            // _waterNoise.SetDomainWarpAmp(30.0f);
        }

        public void DescribeSlice(WorldSliceDescriptionContext context)
        {
            var terrainHeightIn = context.Get(WorldgenAttributes.TerrainHeight);
            var terrainTypeIn = context.Get(WorldgenAttributes.TerrainType);

            var terrainHeight = new ImmutableMap2DBuilder<ushort>(terrainHeightIn);
            var terrainType = new ImmutableMap2DBuilder<TerrainType>(terrainTypeIn);

            var waterHeight = new ImmutableMap2DBuilder<ushort>(ChunkSize);

            _waterNoise.SetSeed((int) context.Seed);
            for (int x = 0; x < ChunkSize; x++)
            {
                for (int z = 0; z < ChunkSize; z++)
                {
                    var noise = _waterNoise.GetNoise(context.Position.X * ChunkSize + x, context.Position.Z * ChunkSize + z);
                    if (noise < Threshold)
                        continue;

                    terrainHeight[x, z] = (ushort) Math.Max(0, terrainHeight[x, z] - Math.Pow(noise - Threshold, 1 / 4f) * 10);
                    terrainType[x, z] = TerrainType.Water;
                    waterHeight[x, z] = terrainHeight[x, z];
                }
            }

            context.Submit(WorldgenAttributes.TerrainHeight, terrainHeight.Build());
            context.Submit(WorldgenAttributes.WaterHeight, waterHeight.Build());
            context.Submit(WorldgenAttributes.TerrainType, terrainType.Build());
        }

        public void PopulateChunk(WorldSliceDescriptor descriptor, IChunk chunk)
        {
            var terrainType = descriptor.Get(WorldgenAttributes.TerrainType);
            var waterHeight = descriptor.Get(WorldgenAttributes.WaterHeight);

            for (int x = 0; x < ChunkSize; x++)
            {
                for (int z = 0; z < ChunkSize; z++)
                {
                    if (terrainType[x, z] != TerrainType.Water)
                        continue;
                    var localHeight = waterHeight[x, z] - chunk.Position.Y * ChunkSize;
                    if (localHeight < 0 || localHeight >= ChunkSize)
                        continue;
                    chunk.SetBlock(new BlockPos(x, (int) localHeight - 1, z), _waterBlock);
                }
            }
        }
    }
}