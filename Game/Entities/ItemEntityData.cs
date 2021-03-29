using DigBuild.Behaviors;
using DigBuild.Engine.Items;
using DigBuild.Engine.Storage;

namespace DigBuild.Entities
{
    internal sealed class ItemEntityData : IData<ItemEntityData>, IItemEntityBehavior
    {
        public ItemInstance Item { get; set; } = ItemInstance.Empty;
        public long JoinWorldTime { get; set; }
        IItemEntity? IItemEntityBehavior.Capability { get; set; }

        public ItemEntityData Copy()
        {
            return new()
            {
                Item = Item,
                JoinWorldTime = JoinWorldTime
            };
        }
    }
}