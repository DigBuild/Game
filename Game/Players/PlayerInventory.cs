using System.Collections.Generic;
using DigBuild.Engine.Items;
using DigBuild.Engine.Items.Inventories;
using DigBuild.Items;

namespace DigBuild.Players
{
    /// <summary>
    /// A player's inventory.
    /// </summary>
    public interface IPlayerInventory
    {
        /// <summary>
        /// The hotbar.
        /// </summary>
        IReadOnlyList<IInventorySlot> Hotbar { get; }
        /// <summary>
        /// The equipment.
        /// </summary>
        IPlayerEquipment Equipment { get; }

        /// <summary>
        /// The current hotbar slot.
        /// </summary>
        uint ActiveHotbarSlot { get; set; }
        /// <summary>
        /// The slot representing the player's currently held item.
        /// </summary>
        IInventorySlot Hand { get; }
        /// <summary>
        /// The item picked by the cursor/controller.
        /// </summary>
        IInventorySlot PickedItem { get; }

        /// <summary>
        /// Cycles the hotbar by the given amount.
        /// </summary>
        /// <param name="amount">The amount</param>
        void CycleHotbar(int amount);
    }

    /// <summary>
    /// A player's inventory.
    /// </summary>
    public sealed class PlayerInventory : IPlayerInventory
    {
        /// <summary>
        /// The size of the hotbar.
        /// </summary>
        public const int HotbarSize = 10;

        private readonly IInventorySlot[] _hotbar = new IInventorySlot[HotbarSize];
        
        public IReadOnlyList<IInventorySlot> Hotbar => _hotbar;
        public IPlayerEquipment Equipment { get; } = new PlayerEquipment();

        public uint ActiveHotbarSlot { get; set; }
        public IInventorySlot Hand => _hotbar[ActiveHotbarSlot];
        public IInventorySlot PickedItem { get; } = new InventorySlot();

        public PlayerInventory()
        {
            for (var i = 0; i < HotbarSize; i++)
                _hotbar[i] = new LockableInventorySlot(new InventorySlot());
        }

        private PlayerInventory(PlayerInventory other)
        {
            for (var i = 0; i < HotbarSize; i++)
                _hotbar[i] = new LockableInventorySlot(new InventorySlot(other._hotbar[i].Item));
            ActiveHotbarSlot = other.ActiveHotbarSlot;
            PickedItem.TrySetItem(other.PickedItem.Item);
        }

        public void CycleHotbar(int amount)
        {
            ActiveHotbarSlot = (uint) ((ActiveHotbarSlot + HotbarSize + (amount % HotbarSize)) % HotbarSize);
        }

        /// <summary>
        /// Creates a deep copy of the inventory.
        /// </summary>
        /// <returns>A deep copy</returns>
        public PlayerInventory Copy()
        {
            return new PlayerInventory(this);
        }
    }
}