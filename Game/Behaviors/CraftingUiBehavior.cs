using System;
using System.Collections.Generic;
using DigBuild.Blocks;
using DigBuild.Client;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Items;
using DigBuild.Items;
using DigBuild.Ui;

namespace DigBuild.Behaviors
{
    public interface ICraftingUiBehavior
    {
        public IReadOnlyList<InventorySlot> ShapedSlots { get; }
        public IReadOnlyList<InventorySlot> ShapelessSlots { get; }
        public InventorySlot CatalystSlot { get; }
        public InventorySlot OutputSlot { get; }
    }

    public sealed class CraftingUiBehavior :
        IBlockBehavior<ICraftingUiBehavior, ICraftingUiBehavior>,
        IItemBehavior<ICraftingUiBehavior, ICraftingUiBehavior>
    {
        public void Build(BlockBehaviorBuilder<ICraftingUiBehavior, ICraftingUiBehavior> block)
        {
            block.Subscribe(OnActivate);
        }

        public void Build(ItemBehaviorBuilder<ICraftingUiBehavior, ICraftingUiBehavior> item)
        {
            item.Subscribe(OnActivate);
        }

        private BlockEvent.Activate.Result OnActivate(IBlockContext context, ICraftingUiBehavior data, BlockEvent.Activate evt, Func<BlockEvent.Activate.Result> next)
        {
            GameWindow.FunnyUi = CraftingUi.Create(new CraftingInventory(data), GameWindow.PickedItemSlot, GameWindow.ItemModels);
            return BlockEvent.Activate.Result.Success;
        }

        private ItemEvent.Activate.Result OnActivate(IPlayerItemContext context, ICraftingUiBehavior data, ItemEvent.Activate evt, Func<ItemEvent.Activate.Result> next)
        {
            GameWindow.FunnyUi = CraftingUi.Create(new CraftingInventory(data), GameWindow.PickedItemSlot, GameWindow.ItemModels);
            return ItemEvent.Activate.Result.Success;
        }

        private sealed class CraftingInventory : ICraftingInventory
        {
            private readonly ICraftingUiBehavior _data;

            public CraftingInventory(ICraftingUiBehavior data)
            {
                _data = data;
            }

            public IReadOnlyList<IInventorySlot> ShapedSlots => _data.ShapedSlots;
            public IReadOnlyList<IInventorySlot> ShapelessSlots => _data.ShapelessSlots;
            public IInventorySlot CatalystSlot => _data.CatalystSlot;
            public IInventorySlot OutputSlot => _data.OutputSlot;
        }
    }
}