using System;
using DigBuild.Behaviors;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Items;
using DigBuild.Engine.Registries;
using DigBuild.Platform.Resource;

namespace DigBuild.Registries
{
    public static class GameItems
    {
        public static Item Dirt { get; private set; } = null!;
        public static Item Grass { get; private set; } = null!;
        public static Item Water { get; private set; } = null!;
        public static Item Stone { get; private set; } = null!;
        public static Item StoneStairs { get; private set; } = null!;
        public static Item Crafter { get; private set; } = null!;

        internal static void Register(RegistryBuilder<Item> registry)
        {
            Dirt = registry.Create(new ResourceName(Game.Domain, "dirt"),
                BlockPlacement(() => GameBlocks.Dirt)
            );
            Grass = registry.Create(new ResourceName(Game.Domain, "grass"),
                BlockPlacement(() => GameBlocks.Grass)
            );
            Water = registry.Create(new ResourceName(Game.Domain, "water"),
                BlockPlacement(() => GameBlocks.Water)
            );
            Stone = registry.Create(new ResourceName(Game.Domain, "stone"),
                BlockPlacement(() => GameBlocks.Stone)
            );
            StoneStairs = registry.Create(new ResourceName(Game.Domain, "stone_stairs"),
                BlockPlacement(() => GameBlocks.StoneStairs)
            );
            Crafter = registry.Create(new ResourceName(Game.Domain, "crafter"),
                BlockPlacement(() => GameBlocks.Crafter)
            );
        }

        private static Action<ItemBuilder> BlockPlacement(Func<Block> blockSupplier)
        {
            return builder => builder.Attach(new PlaceBlockBehavior(blockSupplier));
        }
    }
}