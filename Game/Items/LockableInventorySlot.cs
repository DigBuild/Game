using System;
using DigBuild.Engine.Items;

namespace DigBuild.Items
{
    public class LockableInventorySlot : ILockableInventorySlot
    {
        private readonly InventorySlot _slot;

        public bool IsLocked { get; private set; }

        public LockableInventorySlot(InventorySlot slot)
        {
            _slot = slot;
        }

        public ItemInstance Item => _slot.Item;

        public event Action? Changed
        {
            add => _slot.Changed += value;
            remove => _slot.Changed -= value;
        }

        public bool TrySetItem(ItemInstance value, bool doSet = true)
        {
            if (!IsLocked)
                return _slot.TrySetItem(value, doSet);

            if (value.Count != 0)
                return Item.Equals(value, true, true) && _slot.TrySetItem(value, doSet);

            var empty = Item.Copy();
            empty.Count = 0;
            return _slot.TrySetItem(empty, doSet);

        }

        public void ToggleLocked()
        {
            if (IsLocked)
                IsLocked = false;
            else if (Item.Count == 0)
                throw new Exception("Cannot lock an empty slot.");
            else
                IsLocked = true;
        }
    }
}