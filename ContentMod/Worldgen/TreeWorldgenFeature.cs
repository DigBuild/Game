using System;
using System.Collections.Immutable;
using DigBuild.Content.Registries;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Impl.Worlds;
using DigBuild.Engine.Math;
using DigBuild.Engine.Worldgen;
using DigBuild.Engine.Worlds;

namespace DigBuild.Content.Worldgen
{
    public sealed class TreeWorldgenFeature : IWorldgenFeature
    {
        private const uint ChunkSize = 16;

        private readonly FastNoiseLite _treeDistributionNoise = new();

        private readonly Block _log, _leaves;

        public IImmutableSet<IWorldgenAttribute> InputAttributes => ImmutableHashSet.Create<IWorldgenAttribute>(
            WorldgenAttributes.TerrainType,
            WorldgenAttributes.TerrainHeight,
            WorldgenAttributes.Lushness
        );

        public IImmutableSet<IWorldgenAttribute> OutputAttributes => ImmutableHashSet.Create<IWorldgenAttribute>(
            WorldgenAttributes.HasTree
        );

        public TreeWorldgenFeature(Block log, Block leaves)
        {
            _log = log;
            _leaves = leaves;

            _treeDistributionNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            _treeDistributionNoise.SetFrequency(0.005f);
            _treeDistributionNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
            _treeDistributionNoise.SetFractalOctaves(6);
            _treeDistributionNoise.SetFractalLacunarity(2.0f);
            _treeDistributionNoise.SetFractalGain(1.5f);
        }

        public void DescribeSlice(WorldSliceDescriptionContext context)
        {
            var trees = new ImmutableMap2DBuilder<bool>(ChunkSize);
            var terrainTypeIn = context.Get(WorldgenAttributes.TerrainType);
            var lushnessIn = context.Get(WorldgenAttributes.Lushness);

            _treeDistributionNoise.SetSeed((int) context.Seed);
            for (var x = 0; x < ChunkSize; x++)
            for (var z = 0; z < ChunkSize; z++)
            {
                if (terrainTypeIn[x, z] != TerrainType.Ground)
                    continue;
                if (lushnessIn[x, z] < 0xAF)
                    continue;

                var (nx, nz) = (context.Position.X * ChunkSize + x, context.Position.Z * ChunkSize + z);
                
                var noise = _treeDistributionNoise.GetNoise(nx, nz);
                var neighborNoise = MathF.Max(
                    MathF.Max(
                        _treeDistributionNoise.GetNoise(nx - 1, nz),
                        _treeDistributionNoise.GetNoise(nx + 1, nz)
                    ),
                    MathF.Max(
                        _treeDistributionNoise.GetNoise(nx, nz - 1),
                        _treeDistributionNoise.GetNoise(nx, nz + 1)
                    )
                );
                trees[x, z] = noise > neighborNoise;
            }

            context.Submit(WorldgenAttributes.HasTree, trees.Build());
        }

        public void PopulateChunk(WorldSliceDescriptor descriptor, IChunk chunk)
        {
            var trees = descriptor.Get(WorldgenAttributes.HasTree);
            var height = descriptor.Get(WorldgenAttributes.TerrainHeight);
            
            for (var x = 0; x < ChunkSize; x++)
            for (var z = 0; z < ChunkSize; z++)
            {
                if (!trees[x, z])
                    continue;

                var localHeight = (int) (height[x, z] - chunk.Position.Y * ChunkSize);
                if (localHeight < -1 || localHeight >= ChunkSize)
                    continue;

                var logHeight = localHeight;
                var leavesHeight = localHeight + 1;
                    
                if (logHeight >= 0 && logHeight < ChunkSize)
                    chunk.SetBlock(new ChunkBlockPosition(x, logHeight, z), _log);
                if (leavesHeight >= 0 && leavesHeight < ChunkSize)
                    chunk.SetBlock(new ChunkBlockPosition(x, leavesHeight, z), _leaves);
            }
        }
    }
}