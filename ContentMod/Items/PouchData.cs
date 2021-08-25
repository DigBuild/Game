using DigBuild.Content.Behaviors;
using DigBuild.Engine.Items;
using DigBuild.Engine.Items.Inventories;
using DigBuild.Engine.Storage;

namespace DigBuild.Content.Items
{
    public sealed class PouchData : IData<PouchData>, IPouch
    {
        public IInventorySlot[] Slots { get; } = new IInventorySlot[3 * 5];

        public PouchData()
        {
            for (var i = 0; i < Slots.Length; i++)
                Slots[i] = new InventorySlot();
        }

        public PouchData Copy()
        {
            var data = new PouchData();
            for (var i = 0; i < Slots.Length; i++)
                data.Slots[i].TrySetItem(Slots[i].Item.Copy());
            return data;
        }
    }
}