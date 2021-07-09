using System.Collections.Immutable;
using DigBuild.Content.Registries;
using DigBuild.Engine.Collections;
using DigBuild.Engine.Math;
using DigBuild.Engine.Worldgen;
using DigBuild.Engine.Worlds;
using DigBuild.Registries;
using DigBuild.Worldgen.Biomes;

namespace DigBuild.Content.Worldgen
{
    public class BiomeWorldgenFeature : IWorldgenFeature
    {
        public IImmutableSet<IWorldgenAttribute> InputAttributes => ImmutableHashSet.Create<IWorldgenAttribute>();
        public IImmutableSet<IWorldgenAttribute> OutputAttributes => ImmutableHashSet.Create<IWorldgenAttribute>();

        public void Describe(ChunkDescriptionContext context)
        {
            var biomes = Grid<IBiome>.Builder(WorldDimensions.ChunkSize, GameBiomes.Grassland);
            var scores = Grid<float>.Builder(WorldDimensions.ChunkSize, float.MinValue);

            foreach (var biome in GameRegistries.Biomes.Values)
            {
                var biomeScores = biome.GetScores(context);
                for (var i = 0; i < biomeScores.Size; i++)
                for (var j = 0; j < biomeScores.Size; j++)
                {
                    if (!(biomeScores[i, j] > scores[i, j]))
                        continue;

                    scores[i, j] = biomeScores[i, j];
                    biomes[i, j] = biome;
                }
            }

            context.Submit(WorldgenAttributes.Biome, biomes.Build());
        }

        public void Populate(ChunkDescriptor descriptor, IChunk chunk)
        {
        }
    }
}