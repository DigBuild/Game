using System;
using DigBuild.Engine.Entities;
using DigBuild.Engine.Items;

namespace DigBuild.Entities
{
    public interface IItemEntityBehavior
    {
        public ItemInstance Item { get; set; }

        public long JoinWorldTime { get; set; }

        public IItemEntity? Capability { get; set; }
    }

    public sealed class ItemEntityBehavior : IEntityBehavior<IItemEntityBehavior>
    {
        public void Init(IItemEntityBehavior data)
        {
            data.Item ??= ItemInstance.Empty;
            data.JoinWorldTime = DateTime.Now.Ticks;
            data.Capability = new ItemEntity(data);
        }

        public void Build(EntityBehaviorBuilder<IItemEntityBehavior> entity)
        {
            entity.Add(EntityAttributes.Item, (_, data, _, _) => data.Item);
            entity.Add(EntityAttributes.ItemJoinWorldTime, (_, data, _, _) => data.JoinWorldTime);
            entity.Add(EntityCapabilities.ItemEntity, (_, data, _, _) => data.Capability);
        }

        private sealed class ItemEntity : IItemEntity
        {
            private readonly IItemEntityBehavior _data;

            public ItemEntity(IItemEntityBehavior data)
            {
                _data = data;
            }
            
            public ItemInstance Item
            {
                get => _data.Item;
                set => _data.Item = value;
            }
        }
    }

    public interface IItemEntity
    {
        public ItemInstance Item { get; set; }
    }
}