using DigBuild.Content.Worldgen;
using DigBuild.Engine.Collections;
using DigBuild.Engine.Registries;
using DigBuild.Engine.Worldgen;
using DigBuild.Platform.Resource;
using DigBuild.Worldgen.Biomes;

namespace DigBuild.Content.Registries
{
    public static class WorldgenAttributes
    {
        // Environment constraints

        public static WorldgenAttribute<Grid<float>> Inlandness { get; private set; } = null!;
        public static WorldgenAttribute<Grid<float>> Lushness { get; private set; } = null!;


        // Generation attributes
        
        public static WorldgenAttribute<Grid<IBiome>> Biome { get; private set; } = null!;

        public static WorldgenAttribute<Grid<ushort>> TerrainHeight { get; private set; } = null!;
        public static WorldgenAttribute<Grid<TerrainType>> TerrainType { get; private set; } = null!;
        
        public static WorldgenAttribute<Grid<ushort>> Tree { get; private set; } = null!;

        internal static void Register(RegistryBuilder<IWorldgenAttribute> builder)
        {
            // Environment constraints
            Inlandness = builder.Create<Grid<float>>(new ResourceName(DigBuildGame.Domain, "inlandness"));
            Lushness = builder.Create<Grid<float>>(new ResourceName(DigBuildGame.Domain, "lushness"));
            
            // Generation attributes
            
            Biome = builder.Create<Grid<IBiome>>(new ResourceName(DigBuildGame.Domain, "biome"));

            TerrainHeight = builder.Create<Grid<ushort>>(new ResourceName(DigBuildGame.Domain, "terrain_height"));
            TerrainType = builder.Create<Grid<TerrainType>>(new ResourceName(DigBuildGame.Domain, "terrain_type"));
            
            Tree = builder.Create<Grid<ushort>>(new ResourceName(DigBuildGame.Domain, "tree"));
        }
    }
}