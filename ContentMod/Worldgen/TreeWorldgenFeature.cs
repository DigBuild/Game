using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using DigBuild.Content.Registries;
using DigBuild.Content.Worldgen.Structure;
using DigBuild.Engine.Math;
using DigBuild.Engine.Worldgen;
using DigBuild.Engine.Worlds;

namespace DigBuild.Content.Worldgen
{
    public sealed class TreeWorldgenFeature : IWorldgenFeature
    {
        private const uint ChunkSize = 16;

        private readonly FastNoiseLite _treeDistributionNoise = new();
        
        private readonly IWorldgenStructure _structure;
        private readonly Vector3I _min, _max;

        public IImmutableSet<IWorldgenAttribute> InputAttributes => ImmutableHashSet.Create<IWorldgenAttribute>(
            WorldgenAttributes.TerrainType,
            WorldgenAttributes.TerrainHeight,
            WorldgenAttributes.Lushness
        );

        public IImmutableSet<IWorldgenAttribute> OutputAttributes => ImmutableHashSet.Create<IWorldgenAttribute>(
            WorldgenAttributes.Tree
        );

        public TreeWorldgenFeature(IWorldgenStructure structure)
        {
            _structure = structure;
            _min = structure.Min;
            _max = structure.Max;

            _treeDistributionNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            _treeDistributionNoise.SetFrequency(0.005f);
            _treeDistributionNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
            _treeDistributionNoise.SetFractalOctaves(6);
            _treeDistributionNoise.SetFractalLacunarity(2.0f);
            _treeDistributionNoise.SetFractalGain(1.5f);
        }

        public void DescribeSlice(WorldSliceDescriptionContext context)
        {
            var terrainTypeData = new Dictionary<WorldSliceOffset, ImmutableMap2D<TerrainType>>();
            var lushnessData = new Dictionary<WorldSliceOffset, ImmutableMap2D<byte>>();
            var terrainHeightData = new Dictionary<WorldSliceOffset, ImmutableMap2D<ushort>>();

            TerrainType GetTerrainType(int x, int z)
            {
                var off = new WorldSliceOffset(x >> 4, z >> 4);
                if (!terrainTypeData!.TryGetValue(off, out var data))
                    terrainTypeData[off] = data = context.Get(WorldgenAttributes.TerrainType, off);
                return data[x & 15, z & 15];
            }
            byte GetLushness(int x, int z)
            {
                var off = new WorldSliceOffset(x >> 4, z >> 4);
                if (!lushnessData!.TryGetValue(off, out var data))
                    lushnessData[off] = data = context.Get(WorldgenAttributes.Lushness, off);
                return data[x & 15, z & 15];
            }
            ushort GetTerrainHeight(int x, int z)
            {
                var off = new WorldSliceOffset(x >> 4, z >> 4);
                if (!terrainHeightData!.TryGetValue(off, out var data))
                    terrainHeightData[off] = data = context.Get(WorldgenAttributes.TerrainHeight, off);
                return data[x & 15, z & 15];
            }

            var trees = new ImmutableMap2DBuilder<ushort>((uint) (ChunkSize + Math.Max(_max.X - _min.X, _max.Z - _min.Z)));

            _treeDistributionNoise.SetSeed((int) context.Seed);
            for (var x = _min.X; x < ChunkSize + _max.X; x++)
            for (var z = _min.Z; z < ChunkSize + _max.Z; z++)
            {
                if (GetTerrainType(x, z) != TerrainType.Ground)
                    continue;
                if (GetLushness(x, z) < 0xAF)
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
                var generate = noise > neighborNoise;
                trees[x - _min.X, z - _min.X] = generate ? GetTerrainHeight(x, z) : ushort.MaxValue;
            }

            context.Submit(WorldgenAttributes.Tree, trees.Build());
        }

        public void PopulateChunk(WorldSliceDescriptor descriptor, IChunk chunk)
        {
            var trees = descriptor.Get(WorldgenAttributes.Tree);
            
            for (var x = _min.X; x < ChunkSize + _max.X; x++)
            for (var z = _min.Z; z < ChunkSize + _max.Z; z++)
            {
                var treeY = trees[x - _min.X, z - _min.Z];
                if (treeY == ushort.MaxValue)
                    continue;

                var localHeight = (int) (treeY - chunk.Position.Y * ChunkSize);

                _structure.Place((x, localHeight, z), chunk);
            }
        }
    }
}