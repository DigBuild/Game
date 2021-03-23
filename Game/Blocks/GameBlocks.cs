using System;
using DigBuild.Behaviors;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Items;
using DigBuild.Engine.Math;
using DigBuild.Engine.Registries;
using DigBuild.Items;
using DigBuild.Platform.Resource;
using DigBuild.Recipes;

namespace DigBuild.Blocks
{
    public static class GameBlocks
    {
        public static Block Dirt { get; private set; } = null!;
        public static Block Grass { get; private set; } = null!;
        public static Block Water { get; private set; } = null!;
        public static Block Stone { get; private set; } = null!;

        public static Block TriangleBlock { get; private set; } = null!;

        public static Block Crafter { get; private set; } = null!;

        internal static void Register(RegistryBuilder<Block> registry)
        {
            Dirt = registry.Create(new ResourceName(Game.Domain, "dirt"),
                BlockDrops(() => GameItems.Dirt)
            );
            Grass = registry.Create(new ResourceName(Game.Domain, "grass"), builder =>
                {
                    builder.Attach(new FaceCoveredReplaceBehavior(BlockFace.PosY, () => Dirt));
                },
                BlockDrops(() => GameItems.Grass)
            );
            Water = registry.Create(new ResourceName(Game.Domain, "water"), builder =>
                {
                    builder.Attach(new NoPunchBehavior());
                    builder.Attach(new BoopBehavior());
                },
                BlockDrops(() => GameItems.Water)
            );
            Stone = registry.Create(new ResourceName(Game.Domain, "stone"),
                BlockDrops(() => GameItems.Stone)
            );
            
            TriangleBlock = registry.Create(new ResourceName(Game.Domain, "triangle_block"), builder =>
                {
                    // var countingData = builder.Add<CountingData>();
                    // builder.Attach(new CountingBehavior(), countingData);

                    // builder.Attach(new NoPunchBehavior());

                    var feedData = builder.Add<BlockFeedData>();
                    builder.Attach(new BlockFeedBehavior(), feedData);
                },
                BlockDrops(() => GameItems.TriangleItem)
            );
            
            Crafter = registry.Create(new ResourceName(Game.Domain, "crafter"), builder =>
                {
                    var data = builder.Add<CrafterData>();
                    builder.Attach(new CraftingBehavior(), data);
                },
                BlockDrops(() => GameItems.TriangleItem)
            );
        }

        private static Action<BlockBuilder> BlockDrops(Func<Item> itemSupplier, ushort amount = 1)
        {
            return builder =>
            {
                builder.Attach(new BlockDropsBehavior(() => new ItemInstance(itemSupplier(), amount)));
            };
        }
    }

    public sealed class CrafterData : ICraftingBehavior
    {
        public InventorySlot[] ShapedSlots => throw new NotImplementedException();
        public InventorySlot[] ShapelessSlots => throw new NotImplementedException();
        public InventorySlot CatalystSlot => throw new NotImplementedException();

        public ICraftingRecipe? ActiveRecipe
        {
            set => throw new NotImplementedException();
        }

        public CraftingOutput? ActiveRecipeOutput
        {
            set => throw new NotImplementedException();
        }
    }
}