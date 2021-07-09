using System.Collections.Immutable;
using DigBuild.Content.Registries;
using DigBuild.Engine.Collections;
using DigBuild.Engine.Math;
using DigBuild.Engine.Worldgen;
using DigBuild.Engine.Worlds;

namespace DigBuild.Content.Worldgen
{
    public sealed class ConstraintsWorldgenFeature : IWorldgenFeature
    {
        private const uint ChunkSize = 16;

        private readonly FastNoiseLite _inlandnessNoise = new();

        public IImmutableSet<IWorldgenAttribute> InputAttributes => ImmutableHashSet.Create<IWorldgenAttribute>();
        public IImmutableSet<IWorldgenAttribute> OutputAttributes => ImmutableHashSet.Create<IWorldgenAttribute>();

        public ConstraintsWorldgenFeature()
        {
            _inlandnessNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            _inlandnessNoise.SetFrequency(0.005f);
            _inlandnessNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
            _inlandnessNoise.SetFractalOctaves(3);
            _inlandnessNoise.SetFractalLacunarity(2.0f);
            _inlandnessNoise.SetFractalGain(0.5f);
        }

        public void Describe(ChunkDescriptionContext context)
        {
            var inlandness = Grid<float>.Builder(ChunkSize);
            
            _inlandnessNoise.SetSeed((int) context.Seed);
            for (var x = 0; x < ChunkSize; x++)
            for (var z = 0; z < ChunkSize; z++)
            {
                var (nx, nz) = (context.Position.X * ChunkSize + x, context.Position.Z * ChunkSize + z);
                inlandness[x, z] = _inlandnessNoise.GetNoise(nx, nz) * 0.5f + 0.5f;
            }

            context.Submit(WorldgenAttributes.Inlandness, inlandness.Build());
        }

        public void Populate(ChunkDescriptor descriptor, IChunk chunk)
        {
        }
    }
}