using System.Collections;
using System.Collections.Generic;
using DigBuild.Engine.Items;
using DigBuild.Engine.Items.Inventories;
using DigBuild.Items;
using DigBuild.Registries;

namespace DigBuild.Players
{
    public interface IPlayerEquipment : IEnumerable<IInventorySlot>
    {
        IInventorySlot Helmet { get; }
        IInventorySlot Chestplate { get; }
        IInventorySlot Leggings { get; }
        IInventorySlot Boots { get; }
        
        IInventorySlot EquipTopLeft { get; }
        IInventorySlot EquipTopRight { get; }
        IInventorySlot EquipBottomLeft { get; }
        IInventorySlot EquipBottomRight { get; }

        IEnumerable<IInventorySlot> EquipmentSlots { get; }
    }

    public sealed class PlayerEquipment : IPlayerEquipment
    {
        public IInventorySlot Helmet { get; } = new InventorySlot(IsHelmet);
        public IInventorySlot Chestplate { get; } = new InventorySlot(IsChestplate);
        public IInventorySlot Leggings { get; } = new InventorySlot(IsLeggings);
        public IInventorySlot Boots { get; } = new InventorySlot(IsBoots);

        public IInventorySlot EquipTopLeft { get; } = new InventorySlot(IsEquipment);
        public IInventorySlot EquipTopRight { get; } = new InventorySlot(IsEquipment);
        public IInventorySlot EquipBottomLeft { get; } = new InventorySlot(IsEquipment);
        public IInventorySlot EquipBottomRight { get; } = new InventorySlot(IsEquipment);

        public IEnumerable<IInventorySlot> EquipmentSlots
        {
            get
            {
                yield return EquipTopLeft;
                yield return EquipTopRight;
                yield return EquipBottomLeft;
                yield return EquipBottomRight;
            }
        }

        public IEnumerator<IInventorySlot> GetEnumerator()
        {
            yield return Helmet;
            yield return Chestplate;
            yield return Leggings;
            yield return Boots;

            yield return EquipTopLeft;
            yield return EquipTopRight;
            yield return EquipBottomLeft;
            yield return EquipBottomRight;
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private static bool IsHelmet(IReadOnlyItemInstance item)
        {
            return item.Count == 0 || item.Get(ItemAttributes.Equippable).HasFlag(EquippableFlags.Helmet);
        }

        private static bool IsChestplate(IReadOnlyItemInstance item)
        {
            return item.Count == 0 || item.Get(ItemAttributes.Equippable).HasFlag(EquippableFlags.Chestplate);
        }

        private static bool IsLeggings(IReadOnlyItemInstance item)
        {
            return item.Count == 0 || item.Get(ItemAttributes.Equippable).HasFlag(EquippableFlags.Leggings);
        }

        private static bool IsBoots(IReadOnlyItemInstance item)
        {
            return item.Count == 0 || item.Get(ItemAttributes.Equippable).HasFlag(EquippableFlags.Boots);
        }

        private static bool IsEquipment(IReadOnlyItemInstance item)
        {
            return item.Count == 0 || item.Get(ItemAttributes.Equippable).HasFlag(EquippableFlags.Equipment);
        }
    }
}