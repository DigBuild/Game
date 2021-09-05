using System.Collections.Generic;
using DigBuild.Engine.Items.Inventories;
using DigBuild.Registries;

namespace DigBuild.Items
{
    /// <summary>
    /// Inventory creation helper.
    /// </summary>
    public static class InventoryHelper
    {
        /// <summary>
        /// Creates a new inventory out of the specified slots.
        /// </summary>
        /// <param name="slots">The slots</param>
        /// <returns>The inventory</returns>
        public static SimpleInventory CreateInventory(IEnumerable<IInventorySlot> slots)
        {
            return new SimpleInventory(slots, item => item.Get(GameItemAttributes.MaxStackSize));
        }
    }
}