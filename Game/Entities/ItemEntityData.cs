using System.Numerics;
using DigBuild.Behaviors;
using DigBuild.Engine.Items;
using DigBuild.Engine.Storage;

namespace DigBuild.Entities
{
    internal sealed class ItemEntityData : IData<ItemEntityData>, IItemEntityBehavior, IPhysicalEntityBehavior
    {
        public ItemInstance Item { get; set; } = ItemInstance.Empty;
        public long JoinWorldTime { get; set; }
        IItemEntity? IItemEntityBehavior.Capability { get; set; }

        public Vector3 Position { get; set; }
        IPhysicalEntity? IPhysicalEntityBehavior.Capability { get; set; }

        public ItemEntityData Copy()
        {
            return new()
            {
                Item = Item,
                JoinWorldTime = JoinWorldTime,
                Position = Position
            };
        }
    }
}