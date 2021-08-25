using System.Collections.Generic;
using DigBuild.Engine.Items.Inventories;
using DigBuild.Registries;

namespace DigBuild.Items
{
    public static class InventoryHelper
    {
        public static SimpleInventory CreateInventory(IEnumerable<IInventorySlot> slots)
        {
            return new SimpleInventory(slots, item => item.Get(ItemAttributes.MaxStackSize));
        }
    }
}