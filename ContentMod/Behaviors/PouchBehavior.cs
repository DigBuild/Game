using System;
using DigBuild.Content.Registries;
using DigBuild.Content.Ui;
using DigBuild.Engine.Items;
using DigBuild.Items;
using DigBuild.Players;
using DigBuild.Registries;

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
            item.Add(ItemCapabilities.InventoryExtension, (_, data, _) => new InventoryExtension(data));

            item.Subscribe(OnActivate);
        }

        private ItemEvent.Activate.Result OnActivate(ItemEvent.Activate evt, IPouch data, Func<ItemEvent.Activate.Result> next)
        {
            evt.Player.GameplayController.UiManager.Open(PouchUi.Create(
                data,
                evt.Player.Inventory.PickedItem
            ));

            return ItemEvent.Activate.Result.Success;
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
                Inventory = new SimpleInventory(pouch.Slots);
            }
        }
    }
}