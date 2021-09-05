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
        public static IBiome Pasture { get; private set; } = null!;
        public static IBiome Tundra { get; private set; } = null!;

        // Plateau
        public static IBiome Plateau { get; private set; } = null!;

        // Beach
        public static IBiome Beach { get; private set; } = null!;
        public static IBiome GravelBeach { get; private set; } = null!;

        // Water
        public static IBiome Ocean { get; private set; } = null!;
        public static IBiome FrozenOcean { get; private set; } = null!;

        internal static void Register(RegistryBuilder<IBiome> registry)
        {
            Grassland = registry.Register(new ResourceName(DigBuildGame.Domain, "grassland"), new SimpleBiome
            {
                Constraints = new WorldgenRangeSet
                {
                    { WorldgenAttributes.Inlandness, 0.5f, 0.8f },
                    { WorldgenAttributes.Temperature, 0.4f, 1.0f }
                },
                Attributes = new BiomeAttributeSet
                {
                    { BiomeAttributes.SurfaceBlock, GameBlocks.Grass },
                    { BiomeAttributes.TerrainType, TerrainType.Ground },
                    { BiomeAttributes.TerrainHeightRange, new RangeT<ushort>(12, 16) }
                }
            });

            Pasture = registry.Register(new ResourceName(DigBuildGame.Domain, "pasture"), new SimpleBiome
            {
                Constraints = new WorldgenRangeSet
                {
                    { WorldgenAttributes.Inlandness, 0.5f, 1.0f },
                    { WorldgenAttributes.Temperature, 0.6f, 0.8f }
                },
                Attributes = new BiomeAttributeSet
                {
                    { BiomeAttributes.SurfaceBlock, GameBlocks.Grass },
                    { BiomeAttributes.TerrainType, TerrainType.Ground },
                    { BiomeAttributes.TerrainHeightRange, new RangeT<ushort>(12, 16) }
                }
            });

            Tundra = registry.Register(new ResourceName(DigBuildGame.Domain, "tundra"), new SimpleBiome
            {
                Constraints = new WorldgenRangeSet
                {
                    { WorldgenAttributes.Inlandness, 0.4f, 1.0f },
                    { WorldgenAttributes.Temperature, 0.0f, 0.4f }
                },
                Attributes = new BiomeAttributeSet
                {
                    { BiomeAttributes.SurfaceBlock, GameBlocks.Snow },
                    { BiomeAttributes.TerrainType, TerrainType.Ground },
                    { BiomeAttributes.TerrainHeightRange, new RangeT<ushort>(12, 16) }
                }
            });

            Plateau = registry.Register(new ResourceName(DigBuildGame.Domain, "plateau"), new SimpleBiome
            {
                Constraints = new WorldgenRangeSet
                {
                    { WorldgenAttributes.Inlandness, 0.7f, 1.0f },
                    { WorldgenAttributes.Temperature, 0.5f, 1.0f }
                },
                Attributes = new BiomeAttributeSet
                {
                    { BiomeAttributes.SurfaceBlock, GameBlocks.Grass },
                    { BiomeAttributes.TerrainType, TerrainType.Ground },
                    { BiomeAttributes.TerrainHeightRange, new RangeT<ushort>(25, 30) }
                }
            });
            
            Beach = registry.Register(new ResourceName(DigBuildGame.Domain, "beach"), new SimpleBiome
            {
                Constraints = new WorldgenRangeSet
                {
                    { WorldgenAttributes.Inlandness, 0.4f, 0.5f },
                    { WorldgenAttributes.Temperature, 0.4f, 1.0f }
                },
                Attributes = new BiomeAttributeSet
                {
                    { BiomeAttributes.SurfaceBlock, GameBlocks.Sand },
                    { BiomeAttributes.TerrainType, TerrainType.Unknown },
                    { BiomeAttributes.TerrainHeightRange, new RangeT<ushort>(12, 14) }
                }
            });

            GravelBeach = registry.Register(new ResourceName(DigBuildGame.Domain, "gravel_beach"), new SimpleBiome
            {
                Constraints = new WorldgenRangeSet
                {
                    { WorldgenAttributes.Inlandness, 0.3f, 0.5f },
                    { WorldgenAttributes.Temperature, 0.0f, 0.3f }
                },
                Attributes = new BiomeAttributeSet
                {
                    { BiomeAttributes.SurfaceBlock, GameBlocks.Gravel },
                    { BiomeAttributes.TerrainType, TerrainType.Unknown },
                    { BiomeAttributes.TerrainHeightRange, new RangeT<ushort>(12, 14) }
                }
            });
            
            Ocean = registry.Register(new ResourceName(DigBuildGame.Domain, "ocean"), new SimpleBiome
            {
                Constraints = new WorldgenRangeSet
                {
                    { WorldgenAttributes.Inlandness, 0.0f, 0.45f },
                    { WorldgenAttributes.Temperature, 0.4f, 1.0f }
                },
                Attributes = new BiomeAttributeSet
                {
                    { BiomeAttributes.SurfaceBlock, GameBlocks.Sand },
                    { BiomeAttributes.TerrainType, TerrainType.Water },
                    { BiomeAttributes.TerrainHeightRange, new RangeT<ushort>(2, 12) }
                }
            });

            FrozenOcean = registry.Register(new ResourceName(DigBuildGame.Domain, "frozen_ocean"), new SimpleBiome
            {
                Constraints = new WorldgenRangeSet
                {
                    { WorldgenAttributes.Inlandness, 0.0f, 0.45f },
                    { WorldgenAttributes.Temperature, 0.0f, 0.4f }
                },
                Attributes = new BiomeAttributeSet
                {
                    { BiomeAttributes.SurfaceBlock, GameBlocks.Gravel },
                    { BiomeAttributes.TerrainType, TerrainType.Water },
                    { BiomeAttributes.TerrainHeightRange, new RangeT<ushort>(2, 12) }
                }
            });
        }
    }
}