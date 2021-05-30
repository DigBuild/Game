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

        private readonly SimplexNoise _noise = new(34123413, 0.005f, 2);

        public IImmutableSet<IWorldgenAttribute> InputAttributes => ImmutableHashSet.Create<IWorldgenAttribute>(
            WorldgenAttributes.TerrainType
        );

        public IImmutableSet<IWorldgenAttribute> OutputAttributes => ImmutableHashSet.Create<IWorldgenAttribute>(
            WorldgenAttributes.Lushness
        );
        
        public void DescribeSlice(WorldSliceDescriptionContext context)
        {
            var lushness = Grid<float>.Builder(ChunkSize);
            
            _noise.Seed = context.Seed;
            for (var x = 0; x < ChunkSize; x++)
            for (var z = 0; z < ChunkSize; z++)
            {
                var (nx, nz) = (context.Position.X * ChunkSize + x, context.Position.Z * ChunkSize + z);
                lushness[x, z] = _noise[nx, nz] * 0.5f + 0.5f;
            }

            context.Submit(WorldgenAttributes.Lushness, lushness.Build());
        }

        public void PopulateChunk(WorldSliceDescriptor descriptor, IChunk chunk)
        {
        }
    }
}