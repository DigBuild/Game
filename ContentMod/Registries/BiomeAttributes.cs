using DigBuild.Content.Worldgen;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Math;
using DigBuild.Engine.Registries;
using DigBuild.Platform.Resource;
using DigBuild.Worldgen.Biomes;

namespace DigBuild.Content.Registries
{
    public static class BiomeAttributes
    {
        public static BiomeAttribute<Block> SurfaceBlock { get; private set; } = null!;
        public static BiomeAttribute<TerrainType> TerrainType { get; private set; } = null!;
        public static BiomeAttribute<RangeT<ushort>> TerrainHeightRange { get; private set; } = null!;
        
        internal static void Register(RegistryBuilder<IBiomeAttribute> builder)
        {
            SurfaceBlock = builder.Create<Block>(new ResourceName(DigBuildGame.Domain, "surface_block"));
            TerrainType = builder.Create<TerrainType>(new ResourceName(DigBuildGame.Domain, "terrain_type"));
            TerrainHeightRange = builder.Create<RangeT<ushort>>(new ResourceName(DigBuildGame.Domain, "terrain_height_range"));
        }
    }
}