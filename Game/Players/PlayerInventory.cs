using System.Collections.Generic;
using DigBuild.Engine.Items;

namespace DigBuild.Players
{
    public interface IPlayerInventory
    {
        IReadOnlyList<IInventorySlot> Hotbar { get; }
        IPlayerEquipment Equipment { get; }

        uint ActiveHotbarSlot { get; set; }
        ref IInventorySlot Hand { get; }
        IInventorySlot PickedItem { get; }

        void CycleHotbar(int amount);
    }

    public sealed class PlayerInventory : IPlayerInventory
    {
        public const int HotbarSize = 10;

        private readonly IInventorySlot[] _hotbar = new IInventorySlot[HotbarSize];
        
        public IReadOnlyList<IInventorySlot> Hotbar => _hotbar;
        public IPlayerEquipment Equipment { get; } = new PlayerEquipment();

        public uint ActiveHotbarSlot { get; set; }
        public ref IInventorySlot Hand => ref _hotbar[ActiveHotbarSlot];
        public IInventorySlot PickedItem { get; } = new InventorySlot();

        public PlayerInventory()
        {
            for (var i = 0; i < HotbarSize; i++)
                _hotbar[i] = new InventorySlot();
        }

        private PlayerInventory(PlayerInventory other)
        {
            for (var i = 0; i < HotbarSize; i++)
                _hotbar[i] = new InventorySlot(other._hotbar[i].Item);
            ActiveHotbarSlot = other.ActiveHotbarSlot;
            PickedItem.TrySetItem(other.PickedItem.Item);
        }

        public void CycleHotbar(int amount)
        {
            ActiveHotbarSlot = (uint) ((ActiveHotbarSlot + HotbarSize + (amount % HotbarSize)) % HotbarSize);
        }

        public PlayerInventory Copy()
        {
            return new(this);
        }
    }
}