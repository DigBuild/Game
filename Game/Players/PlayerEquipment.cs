using System.Collections;
using System.Collections.Generic;
using DigBuild.Engine.Items;

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
    }

    public sealed class PlayerEquipment : IPlayerEquipment
    {
        public IInventorySlot Helmet { get; } = new InventorySlot();
        public IInventorySlot Chestplate { get; } = new InventorySlot();
        public IInventorySlot Leggings { get; } = new InventorySlot();
        public IInventorySlot Boots { get; } = new InventorySlot();

        public IInventorySlot EquipTopLeft { get; } = new InventorySlot();
        public IInventorySlot EquipTopRight { get; } = new InventorySlot();
        public IInventorySlot EquipBottomLeft { get; } = new InventorySlot();
        public IInventorySlot EquipBottomRight { get; } = new InventorySlot();
        
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
    }
}