using System;
using System.Collections.Generic;
using DigBuild.Blocks;
using DigBuild.Content.Ui;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Items;
using DigBuild.Ui;

namespace DigBuild.Content.Behaviors
{
    public interface ICraftingUiBehavior
    {
        public IReadOnlyList<InventorySlot> ShapedSlots { get; }
        public IReadOnlyList<InventorySlot> ShapelessSlots { get; }
        public InventorySlot CatalystSlot { get; }
        public InventorySlot OutputSlot { get; }
    }

    public sealed class CraftingUiBehavior :
        IBlockBehavior<ICraftingUiBehavior, ICraftingUiBehavior>
    {
        public void Build(BlockBehaviorBuilder<ICraftingUiBehavior, ICraftingUiBehavior> block)
        {
            block.Subscribe(OnActivate);
        }

        private BlockEvent.Activate.Result OnActivate(BlockEvent.Activate evt, ICraftingUiBehavior data, Func<BlockEvent.Activate.Result> next)
        {
            evt.Player.GameplayController.UiManager.Open(CraftingUi.Create(
                new CraftingInventory(data),
                evt.Player.Inventory.PickedItem
            ));

            return BlockEvent.Activate.Result.Success;
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