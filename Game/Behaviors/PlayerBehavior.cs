using DigBuild.Engine.Entities;
using DigBuild.Engine.Items.Inventories;
using DigBuild.Engine.Storage;
using DigBuild.Items;
using DigBuild.Players;
using DigBuild.Registries;

namespace DigBuild.Behaviors
{
    /// <summary>
    /// The contract and data class for the player behavior.
    /// </summary>
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
                    var inventoryExtension = Inventory.Hand.Item.Get(GameItemCapabilities.InventoryExtension);
                    if (inventoryExtension != null)
                        return inventoryExtension.Inventory;
                }
                foreach (var slot in Inventory.Equipment.EquipmentSlots)
                {
                    if (slot.Item.Count > 0)
                    {
                        var inventoryExtension = slot.Item.Get(GameItemCapabilities.InventoryExtension);
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

    /// <summary>
    /// The player behavior. Contract: <see cref="PlayerBehaviorData"/>.
    /// <para>
    /// Exposes the player's data storage.
    /// </para>
    /// </summary>
    public sealed class PlayerBehavior : IEntityBehavior<PlayerBehaviorData>
    {
        public void Build(EntityBehaviorBuilder<PlayerBehaviorData, PlayerBehaviorData> entity)
        {
            entity.Add(GameEntityCapabilities.PlayerEntity, (_, data, _) => data);
        }
    }

    /// <summary>
    /// The player entity capability interface.
    /// </summary>
    public interface IPlayerEntity
    {
        /// <summary>
        /// The inventory.
        /// </summary>
        PlayerInventory Inventory { get; }
        
        /// <summary>
        /// The state
        /// </summary>
        PlayerState State { get; }
    }
}