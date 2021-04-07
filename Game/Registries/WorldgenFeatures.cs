﻿using DigBuild.Engine.Registries;
using DigBuild.Engine.Worldgen;
using DigBuild.Platform.Resource;
using DigBuild.Worldgen;

namespace DigBuild.Registries
{
    public static class WorldgenFeatures
    {
        public static IWorldgenFeature Terrain { get; private set; } = null!;
        public static IWorldgenFeature Water { get; private set; } = null!;

        internal static void Register(RegistryBuilder<IWorldgenFeature> builder)
        {
            Terrain = builder.Add(new ResourceName(Game.Domain, "terrain"), new TerrainWorldgenFeature(GameBlocks.Dirt, GameBlocks.Grass));
            Water = builder.Add(new ResourceName(Game.Domain, "water"), new WaterWorldgenFeature(GameBlocks.Water));
        }
    }
}