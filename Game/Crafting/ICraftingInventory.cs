using System.Collections.Generic;
using DigBuild.Engine.Items.Inventories;

namespace DigBuild.Crafting
{
    /// <summary>
    /// A crafting inventory.
    /// </summary>
    public interface ICraftingInventory
    {
        /// <summary>
        /// The shaped slots.
        /// </summary>
        public IReadOnlyList<IInventorySlot> ShapedSlots { get; }
        /// <summary>
        /// The shapeless slots.
        /// </summary>
        public IReadOnlyList<IInventorySlot> ShapelessSlots { get; }
        /// <summary>
        /// The catalyst slot.
        /// </summary>
        public IInventorySlot CatalystSlot { get; }
        /// <summary>
        /// The output slot.
        /// </summary>
        public IInventorySlot OutputSlot { get; }
    }
}