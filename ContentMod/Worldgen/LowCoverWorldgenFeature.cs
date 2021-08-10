using System;
using System.Collections.Immutable;
using DigBuild.Content.Registries;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Collections;
using DigBuild.Engine.Math;
using DigBuild.Engine.Worldgen;
using DigBuild.Engine.Worlds;
using DigBuild.Engine.Worlds.Impl;
using DigBuild.Worldgen.Biomes;

namespace DigBuild.Content.Worldgen
{
    public sealed class LowCoverWorldgenFeature : IWorldgenFeature
    {
        private const uint ChunkSize = 16;

        private readonly SimplexNoise _noise;

        public IImmutableSet<IWorldgenAttribute> InputAttributes => ImmutableHashSet.Create<IWorldgenAttribute>(
            WorldgenAttributes.TerrainType,
            WorldgenAttributes.TerrainHeight,
            WorldgenAttributes.Lushness
        );

        public IImmutableSet<IWorldgenAttribute> OutputAttributes => ImmutableHashSet.Create<IWorldgenAttribute>(
        );

        private readonly Block _block;
        private readonly WorldgenAttribute<Grid<bool>> _attribute;
        private readonly float _threshold;
        private readonly Func<IBiome[]> _biomeSupplier;
        
        private ImmutableHashSet<IBiome>? _biomes;

        public LowCoverWorldgenFeature(Block block, WorldgenAttribute<Grid<bool>> attribute, long seed, float threshold, Func<IBiome[]> biomeSupplier)
        {
            _noise = new SimplexNoise(seed, 0.005f, 6, gain: 1.5f);
            _block = block;
            _attribute = attribute;
            _threshold = threshold;
            _biomeSupplier = biomeSupplier;
        }

        public void Describe(ChunkDescriptionContext context)
        {
            var inTerrainType = context.GetExtendedGrid(WorldgenAttributes.TerrainType);
            var inLushness = context.GetExtendedGrid(WorldgenAttributes.Lushness);
            var inBiomes = context.GetExtendedGrid(WorldgenAttributes.Biome);

            _biomes ??= _biomeSupplier().ToImmutableHashSet();

            var coverage = Grid<bool>.Builder(ChunkSize);

            _noise.Seed = context.Seed;
            for (var x = 0; x < ChunkSize; x++)
            for (var z = 0; z < ChunkSize; z++)
            {
                if (inTerrainType[x, z] != TerrainType.Ground)
                    continue;
                if (!_biomes.Contains(inBiomes[x, z]))
                    continue;
                var lushness = inLushness[x, z];
                if (lushness < 0.3f)
                    continue;
                
                var (nx, nz) = (context.Position.X * ChunkSize + x, context.Position.Z * ChunkSize + z);

                var correctedLushness = (lushness - 0.3f) / 0.7f;
                var noise = _noise[nx, nz] * (1 - (1 - correctedLushness) * (1 - correctedLushness) * (1 - correctedLushness));
                coverage[x, z] = noise > _threshold;
            }

            context.Submit(_attribute, coverage.Build());
        }

        public void Populate(ChunkDescriptor descriptor, IChunk chunk)
        {
            var coverage = descriptor.Get(_attribute);
            var height = descriptor.Get(WorldgenAttributes.TerrainHeight);
            
            for (var x = 0; x < ChunkSize; x++)
            for (var z = 0; z < ChunkSize; z++)
            {
                var y = height[x, z];
                if (y == 0)
                    continue;

                if (coverage[x, z])
                    chunk.SetBlock(new ChunkBlockPos(x, y, z), _block);
            }
        }
    }
}