using System.Collections.Generic;
using DigBuild.Engine.Items;

namespace DigBuild.Players
{
    public sealed class PlayerInventory
    {
        public const int HotbarSize = 8;

        private readonly IInventorySlot[] _hotbar = new IInventorySlot[HotbarSize];
        
        public IReadOnlyList<IInventorySlot> Hotbar => _hotbar;
        public uint ActiveHotbarSlot { get; set; } = 0;
        public ref IInventorySlot Hand => ref _hotbar[ActiveHotbarSlot];

        public IInventorySlot PickedItem { get; } = new InventorySlot();

        public PlayerInventory()
        {
            for (var i = 0; i < HotbarSize; i++)
                _hotbar[i] = new InventorySlot();
        }

        public void CycleHotbar(int amount)
        {
            ActiveHotbarSlot = (uint) ((ActiveHotbarSlot + HotbarSize + (amount % HotbarSize)) % HotbarSize);
        }
    }
}