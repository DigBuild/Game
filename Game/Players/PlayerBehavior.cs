using DigBuild.Behaviors;
using DigBuild.Engine.Entities;
using DigBuild.Engine.Items.Inventories;
using DigBuild.Engine.Storage;
using DigBuild.Items;
using DigBuild.Registries;

namespace DigBuild.Players
{
    public sealed class PlayerBehaviorData : IData<PlayerBehaviorData>, IPlayerEntity, IItemPickup
    {
        public PlayerInventory Inventory { get; init; } = new();
        public PlayerState State { get; init; } = new();
        
        // IItemPickup
        public bool InWorld { get; set; }

        public IInventory PickupTarget
        {
            get {
                if (Inventory.Hand.Item.Count > 0)
                {
                    var inventoryExtension = Inventory.Hand.Item.Get(ItemCapabilities.InventoryExtension);
                    if (inventoryExtension != null)
                        return inventoryExtension.Inventory;
                }
                foreach (var slot in Inventory.Equipment.EquipmentSlots)
                {
                    if (slot.Item.Count > 0)
                    {
                        var inventoryExtension = slot.Item.Get(ItemCapabilities.InventoryExtension);
                        if (inventoryExtension != null)
                            return inventoryExtension.Inventory;
                    }
                }
                return InventoryHelper.CreateInventory(Inventory.Hotbar);
            }
        }

        public PlayerBehaviorData Copy()
        {
            return new PlayerBehaviorData
            {
                Inventory = Inventory.Copy(),
                State = State.Copy()
            };
        }
    }

    public sealed class PlayerBehavior : IEntityBehavior<PlayerBehaviorData>
    {
        public void Build(EntityBehaviorBuilder<PlayerBehaviorData, PlayerBehaviorData> entity)
        {
            entity.Add(EntityCapabilities.PlayerEntity, (_, data, _) => data);
        }
    }

    public interface IPlayerEntity
    {
        PlayerInventory Inventory { get; }

        PlayerState State { get; }
    }
}