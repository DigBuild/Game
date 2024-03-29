﻿using DigBuild.Content.Worldgen;
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
        public static WorldgenAttribute<Grid<float>> Temperature { get; private set; } = null!;
        public static WorldgenAttribute<Grid<float>> Lushness { get; private set; } = null!;


        // Generation attributes
        
        public static WorldgenAttribute<Grid<IBiome>> Biome { get; private set; } = null!;

        public static WorldgenAttribute<Grid<ushort>> TerrainHeight { get; private set; } = null!;
        public static WorldgenAttribute<Grid<TerrainType>> TerrainType { get; private set; } = null!;
        
        public static WorldgenAttribute<Grid<ushort>> Tree { get; private set; } = null!;
        
        public static WorldgenAttribute<Grid<bool>> TallGrass { get; private set; } = null!;
        public static WorldgenAttribute<Grid<bool>> Barley { get; private set; } = null!;

        internal static void Register(RegistryBuilder<IWorldgenAttribute> builder)
        {
            // Environment constraints
            Inlandness = builder.Register<Grid<float>>(new ResourceName(DigBuildGame.Domain, "inlandness"));
            Temperature = builder.Register<Grid<float>>(new ResourceName(DigBuildGame.Domain, "temperature"));
            Lushness = builder.Register<Grid<float>>(new ResourceName(DigBuildGame.Domain, "lushness"));
            
            // Generation attributes
            
            Biome = builder.Register<Grid<IBiome>>(new ResourceName(DigBuildGame.Domain, "biome"));

            TerrainHeight = builder.Register<Grid<ushort>>(new ResourceName(DigBuildGame.Domain, "terrain_height"));
            TerrainType = builder.Register<Grid<TerrainType>>(new ResourceName(DigBuildGame.Domain, "terrain_type"));
            
            Tree = builder.Register<Grid<ushort>>(new ResourceName(DigBuildGame.Domain, "tree"));
            
            TallGrass = builder.Register<Grid<bool>>(new ResourceName(DigBuildGame.Domain, "tall_grass"));
            Barley = builder.Register<Grid<bool>>(new ResourceName(DigBuildGame.Domain, "barley"));
        }
    }
}