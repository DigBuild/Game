using System;
using DigBuild.Blocks;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Items;
using DigBuild.Engine.Reg;
using DigBuild.Platform.Resource;

namespace DigBuild.Items
{
    public static class GameItems
    {
        public static Item Dirt { get; private set; } = null!;
        public static Item Grass { get; private set; } = null!;
        public static Item Water { get; private set; } = null!;
        public static Item Stone { get; private set; } = null!;
        public static Item TriangleItem { get; private set; } = null!;
        public static Item Crafter { get; private set; } = null!;

        public static Item CountingItem { get; private set; } = null!;

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
            TriangleItem = registry.Create(new ResourceName(Game.Domain, "triangle_Item"),
                BlockPlacement(() => GameBlocks.TriangleBlock)
            );
            Crafter = registry.Create(new ResourceName(Game.Domain, "crafter"),
                BlockPlacement(() => GameBlocks.Crafter)
            );

            CountingItem = registry.Create(new ResourceName(Game.Domain, "counting_item"), builder =>
                {
                    var data = builder.Add<CountingData>();
                    builder.Attach(new CountingBehavior(), data);
                }
            );
        }

        private static Action<ItemBuilder> BlockPlacement(Func<Block> blockSupplier)
        {
            return builder => builder.Attach(new BlockPlaceBehavior(blockSupplier));
        }
    }
}