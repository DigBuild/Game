using System;
using System.Collections.Generic;
using DigBuild.Blocks;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Items;
using DigBuild.Ui;

namespace DigBuild.Behaviors
{
    public interface IReadOnlyCraftingUiBehavior
    {
        public IReadOnlyList<IReadOnlyInventorySlot> ShapedSlots { get; }
        public IReadOnlyList<IReadOnlyInventorySlot> ShapelessSlots { get; }
        public IReadOnlyInventorySlot CatalystSlot { get; }
        public IReadOnlyInventorySlot OutputSlot { get; }
    }

    public interface ICraftingUiBehavior : IReadOnlyCraftingUiBehavior
    {
        public new IReadOnlyList<InventorySlot> ShapedSlots { get; }
        public new IReadOnlyList<InventorySlot> ShapelessSlots { get; }
        public new InventorySlot CatalystSlot { get; }
        public new InventorySlot OutputSlot { get; }

        IReadOnlyList<IReadOnlyInventorySlot> IReadOnlyCraftingUiBehavior.ShapedSlots => ShapedSlots;
        IReadOnlyList<IReadOnlyInventorySlot> IReadOnlyCraftingUiBehavior.ShapelessSlots => ShapelessSlots;
        IReadOnlyInventorySlot IReadOnlyCraftingUiBehavior.CatalystSlot => CatalystSlot;
        IReadOnlyInventorySlot IReadOnlyCraftingUiBehavior.OutputSlot => OutputSlot;
    }

    public sealed class CraftingUiBehavior : IBlockBehavior<IReadOnlyCraftingUiBehavior, ICraftingUiBehavior>
    {
        public void Build(BlockBehaviorBuilder<IReadOnlyCraftingUiBehavior, ICraftingUiBehavior> block)
        {
            block.Subscribe(OnActivate);
        }

        private BlockEvent.Activate.Result OnActivate(IBlockContext context, ICraftingUiBehavior data, BlockEvent.Activate evt, Func<BlockEvent.Activate.Result> next)
        {
            GameWindow.FunnyUi = CraftingUi.Create(new CraftingInventory(data), GameWindow.PickedItemSlot, GameWindow.ItemModels);
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