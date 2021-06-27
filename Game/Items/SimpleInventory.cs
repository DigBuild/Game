using System;
using System.Collections.Generic;
using System.Linq;
using DigBuild.Engine.Items;
using DigBuild.Registries;

namespace DigBuild.Items
{
    public sealed class SimpleInventory : IInventory
    {
        private readonly IInventorySlot[] _slots;

        public SimpleInventory(IEnumerable<IInventorySlot> slots)
        {
            _slots = slots.ToArray();
        }

        public IInventoryTransaction BeginTransaction()
        {
            return new Transaction(_slots);
        }

        private sealed class Transaction : IInventoryTransaction
        {
            private static readonly Func<IReadOnlyItemInstance, bool> AnyItem = _ => true;

            private readonly IInventorySlot[] _slots;
            private readonly ItemInstance?[] _items;
            private readonly bool[] _modified;

            public Transaction(IInventorySlot[] slots)
            {
                _slots = slots;
                _items = new ItemInstance?[slots.Length];
                _modified = new bool[slots.Length];
            }

            private ItemInstance Get(uint slot)
            {
                var item = _items[slot];
                if (item != null)
                    return item;
                return _items[slot] = _slots[slot].Item;
            }

            public ItemInstance Insert(ItemInstance item)
            {
                for (var i = 0u; i < _slots.Length; i++)
                {
                    var current = Get(i);
                    if (current.Count == 0)
                    {
                        _items[i] = item.Copy();
                        _modified[i] = true;
                        return ItemInstance.Empty;
                    }

                    if (!current.Equals(item, true))
                        continue;
                    if (!_slots[i].TrySetItem(item, false))
                        continue;

                    var maxStackSize = current.Get(ItemAttributes.MaxStackSize);
                    if (current.Count == maxStackSize)
                        continue;

                    var totalStackSize = (ushort) (current.Count + item.Count);

                    if (maxStackSize >= totalStackSize)
                    {
                        _items[i] = item.Copy();
                        _items[i]!.Count = totalStackSize;
                        _modified[i] = true;
                        return ItemInstance.Empty;
                    }

                    _items[i] = current.Copy();
                    _items[i]!.Count = maxStackSize;
                    _modified[i] = true;

                    item = item.Copy();
                    item.Count = (ushort) (totalStackSize - maxStackSize);
                }

                return item;
            }

            public ItemInstance Extract(ushort amount, Func<IReadOnlyItemInstance, bool>? test = null)
            {
                test ??= AnyItem;

                var result = ItemInstance.Empty;

                for (var i = 0u; i < _slots.Length; i++)
                {
                    var current = Get(i);
                    if (current.Count == 0)
                        continue;

                    if (result.Count != 0 ? !result.Equals(current, true) : !test(current))
                        continue;

                    var remaining = amount - result.Count;

                    if (current.Count <= remaining)
                    {
                        if (result.Count == 0)
                            result = current.Copy();
                        else
                            result.Count += current.Count;
                        _items[i] = ItemInstance.Empty;
                        _modified[i] = true;

                        if (current.Count == remaining)
                            break;
                        continue;
                    }

                    result = current.Copy();
                    result.Count = amount;

                    _items[i] = current.Copy();
                    _items[i]!.Count -= (ushort) remaining;
                    _modified[i] = true;

                    break;
                }
                
                return result;
            }

            public void Checkpoint()
            {
                throw new NotImplementedException();
            }

            public void Rollback()
            {
                throw new NotImplementedException();
            }

            public void Commit()
            {
                for (var i = 0u; i < _slots.Length; i++)
                    if (_modified[i])
                        _slots[i].TrySetItem(_items[i]!);
            }
        }
    }
}