using System;
using System.Linq;
using DigBuild.Content.Ui;
using DigBuild.Engine.Items;
using DigBuild.Engine.Items.Inventories;
using DigBuild.Items;
using DigBuild.Players;
using DigBuild.Registries;
using DigBuild.Ui;

namespace DigBuild.Content.Behaviors
{
    public interface IPouch
    {
        IInventorySlot[] Slots { get; }
    }

    public sealed class PouchBehavior : IItemBehavior<IPouch>
    {
        public void Build(ItemBehaviorBuilder<IPouch, IPouch> item)
        {
            item.Add(GameItemCapabilities.InventoryExtension, (_, data, _) => new InventoryExtension(data));

            item.Subscribe(OnActivate);
            item.Subscribe(OnEquipmentActivate);
        }

        private ItemEvent.Activate.Result OnActivate(ItemEvent.Activate evt, IPouch data, Func<ItemEvent.Activate.Result> next)
        {
            evt.Player.GameplayController.UiManager.Open(PouchUi.Create(
                data,
                evt.Player.Inventory.PickedItem
            ));

            return ItemEvent.Activate.Result.Success;
        }

        private void OnEquipmentActivate(ItemEvent.EquipmentActivate evt, IPouch data, Action next)
        {
            var uiManager = evt.Player.GameplayController.UiManager;

            if (uiManager.Uis.First() is not GameHud)
                return;
            
            evt.Player.GameplayController.UiManager.Open(PouchUi.Create(
                data,
                evt.Player.Inventory.PickedItem
            ));
        }

        public bool Equals(IPouch first, IPouch second)
        {
            for (var i = 0; i < first.Slots.Length; i++)
                if (!first.Slots[i].Item.Equals(second.Slots[i].Item))
                    return false;
            return true;
        }

        private sealed class InventoryExtension : IPlayerInventoryExtension
        {
            public IInventory Inventory { get; }

            public InventoryExtension(IPouch pouch)
            {
                Inventory = InventoryHelper.CreateInventory(pouch.Slots);
            }
        }
    }
}