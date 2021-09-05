using System.Collections;
using System.Collections.Generic;
using DigBuild.Engine.Items;
using DigBuild.Engine.Items.Inventories;
using DigBuild.Items;
using DigBuild.Registries;

namespace DigBuild.Players
{
    /// <summary>
    /// The player's equipment.
    /// </summary>
    public interface IPlayerEquipment : IEnumerable<IInventorySlot>
    {
        /// <summary>
        /// The helmet slot.
        /// </summary>
        IInventorySlot Helmet { get; }
        /// <summary>
        /// The chestplate slot.
        /// </summary>
        IInventorySlot Chestplate { get; }
        /// <summary>
        /// The leggings slot.
        /// </summary>
        IInventorySlot Leggings { get; }
        /// <summary>
        /// The boots slot.
        /// </summary>
        IInventorySlot Boots { get; }
        
        /// <summary>
        /// The top left general-purpose equipment slot.
        /// </summary>
        IInventorySlot EquipTopLeft { get; }
        /// <summary>
        /// The top right general-purpose equipment slot.
        /// </summary>
        IInventorySlot EquipTopRight { get; }
        /// <summary>
        /// The bottom left general-purpose equipment slot.
        /// </summary>
        IInventorySlot EquipBottomLeft { get; }
        /// <summary>
        /// The bottom right general-purpose equipment slot.
        /// </summary>
        IInventorySlot EquipBottomRight { get; }

        /// <summary>
        /// An enumeration of all the equipment slots.
        /// </summary>
        IEnumerable<IInventorySlot> EquipmentSlots { get; }
    }

    /// <summary>
    /// A player's equipment.
    /// </summary>
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
            return item.Count == 0 || item.Get(GameItemAttributes.Equippable).HasFlag(EquippableFlags.Helmet);
        }

        private static bool IsChestplate(IReadOnlyItemInstance item)
        {
            return item.Count == 0 || item.Get(GameItemAttributes.Equippable).HasFlag(EquippableFlags.Chestplate);
        }

        private static bool IsLeggings(IReadOnlyItemInstance item)
        {
            return item.Count == 0 || item.Get(GameItemAttributes.Equippable).HasFlag(EquippableFlags.Leggings);
        }

        private static bool IsBoots(IReadOnlyItemInstance item)
        {
            return item.Count == 0 || item.Get(GameItemAttributes.Equippable).HasFlag(EquippableFlags.Boots);
        }

        private static bool IsEquipment(IReadOnlyItemInstance item)
        {
            return item.Count == 0 || item.Get(GameItemAttributes.Equippable).HasFlag(EquippableFlags.Equipment);
        }
    }
}