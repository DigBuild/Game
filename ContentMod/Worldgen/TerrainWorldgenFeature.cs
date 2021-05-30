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
        private static readonly RangeT<ushort> DefaultHeightRange = new(2, 30);

        private readonly SimplexNoise _noise = new(152351234, 0.001f, 4, 2.4f, 0.4f);

        private readonly Block _terrainBlock, _surfaceBlock;

        public IImmutableSet<IWorldgenAttribute> InputAttributes => ImmutableHashSet.Create<IWorldgenAttribute>();

        public IImmutableSet<IWorldgenAttribute> OutputAttributes => ImmutableHashSet.Create<IWorldgenAttribute>(
            WorldgenAttributes.TerrainHeight, WorldgenAttributes.TerrainType
        );

        public TerrainWorldgenFeature(Block terrainBlock, Block surfaceBlock)
        {
            _terrainBlock = terrainBlock;
            _surfaceBlock = surfaceBlock;
        }

        public void DescribeSlice(WorldSliceDescriptionContext context)
        {
            var inBiome = context.Get(WorldgenAttributes.Biome);

            var height = Grid<ushort>.Builder(ChunkSize);
            var terrainType = Grid<TerrainType>.Builder(ChunkSize, TerrainType.Ground);

            _noise.Seed = context.Seed;
            for (var x = 0; x < ChunkSize; x++)
            for (var z = 0; z < ChunkSize; z++)
            {
                var noise = _noise[context.Position.X * ChunkSize + x, context.Position.Z * ChunkSize + z] * 0.5f + 0.5f;
                var biome = inBiome[x, z];

                var heightRange = biome.Get(BiomeAttributes.TerrainHeightRange, DefaultHeightRange);
                var minHeight = heightRange.Start;
                var heightDelta = heightRange.End - heightRange.Start;

                height[x, z] = (ushort) (minHeight + heightDelta * noise);
                terrainType[x, z] = biome.Get(BiomeAttributes.TerrainType, TerrainType.Unknown);
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
                    var surfaceBlock = biome.Get(BiomeAttributes.SurfaceBlock, _surfaceBlock);
                    for (var y = 0; y < localHeight - 1; y++)
                        chunk.SetBlock(new ChunkBlockPos(x, y, z), _terrainBlock);
                    
                    chunk.SetBlock(new ChunkBlockPos(x, (int) (localHeight - 1), z), localHeight == relativeHeight ? surfaceBlock : _terrainBlock);
                }
            }
        }
    }
}