using System;
using System.Collections.Immutable;
using DigBuild.Content.Registries;
using DigBuild.Content.Worldgen.Structure;
using DigBuild.Engine.Collections;
using DigBuild.Engine.Math;
using DigBuild.Engine.Worldgen;
using DigBuild.Engine.Worlds;

namespace DigBuild.Content.Worldgen
{
    public sealed class TreeWorldgenFeature : IWorldgenFeature
    {
        private const uint ChunkSize = 16;

        private readonly SimplexNoise _noise = new(5143513241324, 0.005f, 6, gain: 1.5f);
        
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
        }

        public void Describe(ChunkDescriptionContext context)
        {
            var inTerrainType = context.GetExtendedGrid(WorldgenAttributes.TerrainType);
            var inLushness = context.GetExtendedGrid(WorldgenAttributes.Lushness);
            var inTerrainHeight = context.GetExtendedGrid(WorldgenAttributes.TerrainHeight);

            var trees = Grid<ushort>.Builder((uint) (ChunkSize + Math.Max(_max.X - _min.X, _max.Z - _min.Z)));

            _noise.Seed = context.Seed;
            for (var x = _min.X; x < ChunkSize + _max.X; x++)
            for (var z = _min.Z; z < ChunkSize + _max.Z; z++)
            {
                if (inTerrainType[x, z] != TerrainType.Ground)
                    continue;
                if (inLushness[x, z] < 0.7f)
                    continue;
                
                var (nx, nz) = (context.Position.X * ChunkSize + x, context.Position.Z * ChunkSize + z);
                
                var noise = _noise[nx, nz];
                var neighborNoise = MathF.Max(
                    MathF.Max(
                        _noise[nx - 1, nz],
                        _noise[nx + 1, nz]
                    ),
                    MathF.Max(
                        _noise[nx, nz - 1],
                        _noise[nx, nz + 1]
                    )
                );
                var generate = noise > neighborNoise;
                trees[x - _min.X, z - _min.X] = generate ? inTerrainHeight[x, z] : ushort.MaxValue;
            }

            context.Submit(WorldgenAttributes.Tree, trees.Build());
        }

        public void Populate(ChunkDescriptor descriptor, IChunk chunk)
        {
            var trees = descriptor.Get(WorldgenAttributes.Tree);
            
            for (var x = _min.X; x < ChunkSize + _max.X; x++)
            for (var z = _min.Z; z < ChunkSize + _max.Z; z++)
            {
                var treeY = trees[x - _min.X, z - _min.Z];
                if (treeY == ushort.MaxValue)
                    continue;
                if (treeY == 0)
                    continue;
                
                _structure.Place((x, treeY, z), chunk);
            }
        }
    }
}