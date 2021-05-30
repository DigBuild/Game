using DigBuild.Content.Worldgen;
using DigBuild.Engine.Math;
using DigBuild.Engine.Registries;
using DigBuild.Platform.Resource;
using DigBuild.Worldgen.Biomes;

namespace DigBuild.Content.Registries
{
    public class GameBiomes
    {
        // Plains
        public static IBiome Grassland { get; private set; } = null!;

        // Beach
        public static IBiome Beach { get; private set; } = null!;

        // Water
        public static IBiome Ocean { get; private set; } = null!;

        internal static void Register(RegistryBuilder<IBiome> registry)
        {
            Grassland = registry.Add(new ResourceName(DigBuildGame.Domain, "grassland"), new SimpleBiome
            {
                Constraints = new WorldgenRangeSet
                {
                    { WorldgenAttributes.Inlandness, 0.5f, 1.0f }
                },
                Attributes = new BiomeAttributeSet
                {
                    { BiomeAttributes.SurfaceBlock, GameBlocks.Grass },
                    { BiomeAttributes.TerrainType, TerrainType.Ground },
                    { BiomeAttributes.TerrainHeightRange, new RangeT<ushort>(12, 16) }
                }
            });

            Beach = registry.Add(new ResourceName(DigBuildGame.Domain, "beach"), new SimpleBiome
            {
                Constraints = new WorldgenRangeSet
                {
                    { WorldgenAttributes.Inlandness, 0.4f, 0.5f }
                },
                Attributes = new BiomeAttributeSet
                {
                    { BiomeAttributes.SurfaceBlock, GameBlocks.Sand },
                    { BiomeAttributes.TerrainType, TerrainType.Unknown },
                    { BiomeAttributes.TerrainHeightRange, new RangeT<ushort>(12, 14) }
                }
            });

            Ocean = registry.Add(new ResourceName(DigBuildGame.Domain, "ocean"), new SimpleBiome
            {
                Constraints = new WorldgenRangeSet
                {
                    { WorldgenAttributes.Inlandness, 0.0f, 0.4f }
                },
                Attributes = new BiomeAttributeSet
                {
                    { BiomeAttributes.SurfaceBlock, GameBlocks.Stone },
                    { BiomeAttributes.TerrainType, TerrainType.Water },
                    { BiomeAttributes.TerrainHeightRange, new RangeT<ushort>(2, 12) }
                }
            });
        }
    }
}