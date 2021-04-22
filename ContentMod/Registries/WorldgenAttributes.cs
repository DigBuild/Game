using DigBuild.Content.Worldgen;
using DigBuild.Engine.Registries;
using DigBuild.Engine.Worldgen;
using DigBuild.Platform.Resource;

namespace DigBuild.Content.Registries
{
    public static class WorldgenAttributes
    {
        public static WorldgenAttribute<ImmutableMap2D<ushort>> TerrainHeight { get; private set; } = null!;
        public static WorldgenAttribute<ImmutableMap2D<TerrainType>> TerrainType { get; private set; } = null!;

        public static WorldgenAttribute<ImmutableMap2D<ushort>> WaterHeight { get; private set; } = null!;
        

        public static WorldgenAttribute<ImmutableMap2D<byte>> Lushness { get; private set; } = null!;
        public static WorldgenAttribute<ImmutableMap2D<bool>> HasTree { get; private set; } = null!;

        internal static void Register(RegistryBuilder<IWorldgenAttribute> builder)
        {
            TerrainHeight = builder.Create<ImmutableMap2D<ushort>>(new ResourceName(Game.Domain, "terrain_height"));
            TerrainType = builder.Create<ImmutableMap2D<TerrainType>>(new ResourceName(Game.Domain, "terrain_type"));

            WaterHeight = builder.Create<ImmutableMap2D<ushort>>(new ResourceName(Game.Domain, "water_height"));
            
            Lushness = builder.Create<ImmutableMap2D<byte>>(new ResourceName(Game.Domain, "lushness"));
            HasTree = builder.Create<ImmutableMap2D<bool>>(new ResourceName(Game.Domain, "has_tree"));
        }
    }
}