using System;
using DigBuild.Engine.Entities;
using DigBuild.Engine.Items;
using DigBuild.Engine.Serialization;
using DigBuild.Engine.Storage;
using DigBuild.Registries;

namespace DigBuild.Behaviors
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

        public void Build(EntityBehaviorBuilder<IItemEntityBehavior, IItemEntityBehavior> entity)
        {
            entity.Add(EntityAttributes.Item, (_, data, _) => data.Item);
            entity.Add(EntityAttributes.ItemJoinWorldTime, (_, data, _) => data.JoinWorldTime);
            entity.Add(EntityCapabilities.ItemEntity, (_, data, _) => data.Capability);
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

        public static ISerdes<ItemEntityData> Serdes { get; } =
            new CompositeSerdes<ItemEntityData>()
            {
                {1u, d => d.Item, ItemInstance.Serdes},
                {2u, d => d.JoinWorldTime, UnmanagedSerdes<long>.NotNull}
            };
    }

    public interface IItemEntity
    {
        public ItemInstance Item { get; set; }
    }
}