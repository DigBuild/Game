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
        
        private readonly SimplexNoise _inlandnessNoise = new(12315612135, 0.005f, 3, gain: 0.5f);
        private readonly SimplexNoise _temperatureNoise = new(522318133, 0.004f, 2, gain: 0.7f);
        private readonly SimplexNoise _lushnessNoise = new(34123413, 0.005f, 2);
        
        public void Describe(ChunkDescriptionContext context)
        {
            var inlandness = Grid<float>.Builder(ChunkSize);
            var temperature = Grid<float>.Builder(ChunkSize);
            var lushness = Grid<float>.Builder(ChunkSize);
            
            _inlandnessNoise.Seed = context.Seed;
            _temperatureNoise.Seed = context.Seed;
            _lushnessNoise.Seed = context.Seed;
            for (var x = 0; x < ChunkSize; x++)
            for (var z = 0; z < ChunkSize; z++)
            {
                var (nx, nz) = (context.Position.X * ChunkSize + x, context.Position.Z * ChunkSize + z);
                inlandness[x, z] = _inlandnessNoise[nx, nz] * 0.5f + 0.5f;
                temperature[x, z] = _temperatureNoise[nx, nz] * 0.5f + 0.5f;
                lushness[x, z] = _lushnessNoise[nx, nz] * 0.5f + 0.5f;
            }

            context.Submit(WorldgenAttributes.Inlandness, inlandness.Build());
            context.Submit(WorldgenAttributes.Temperature, temperature.Build());
            context.Submit(WorldgenAttributes.Lushness, lushness.Build());
        }

        public void Populate(ChunkDescriptor descriptor, IChunk chunk)
        {
        }
    }
}