using System.Collections.Generic;
using DigBuild.Engine.Items;
using DigBuild.Engine.Items.Inventories;

namespace DigBuild.Ui
{
    public interface ICraftingInventory
    {
        public IReadOnlyList<IInventorySlot> ShapedSlots { get; }
        public IReadOnlyList<IInventorySlot> ShapelessSlots { get; }
        public IInventorySlot CatalystSlot { get; }
        public IInventorySlot OutputSlot { get; }
    }
}