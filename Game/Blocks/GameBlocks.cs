using System;
using System.Collections.Generic;
using DigBuild.Behaviors;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Items;
using DigBuild.Engine.Math;
using DigBuild.Engine.Physics;
using DigBuild.Engine.Registries;
using DigBuild.Engine.Storage;
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
                    builder.Attach(new FaceCoveredReplaceBehavior(Direction.PosY, () => Dirt));
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
                    builder.Attach(new CraftingUiBehavior(), data);
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

    public sealed class CrafterData : IData<CrafterData>, ICraftingBehavior, ICraftingUiBehavior
    {
        public IReadOnlyList<InventorySlot> ShapedSlots { get; } = new InventorySlot[]{new(), new(), new(), new(), new(), new(), new()};
        public IReadOnlyList<InventorySlot> ShapelessSlots { get; } = new InventorySlot[]{new(), new(), new(), new()};
        public InventorySlot CatalystSlot { get; } = new();
        public InventorySlot OutputSlot { get; } = new();

        public ICraftingRecipe? ActiveRecipe
        {
            set {
            }
        }

        public CraftingOutput? ActiveRecipeOutput
        {
            set => OutputSlot.Item = value?.Output ?? ItemInstance.Empty;
        }

        public CrafterData Copy()
        {
            var copy = new CrafterData();
            for (var i = 0; i < ShapedSlots.Count; i++)
                copy.ShapedSlots[i].Item = ShapedSlots[i].Item.Copy();
            return copy;
        }
    }
}