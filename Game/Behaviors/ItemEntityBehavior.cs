using System;
using DigBuild.Engine.BuiltIn;
using DigBuild.Engine.Entities;
using DigBuild.Engine.Items;
using DigBuild.Engine.Render.Models;
using DigBuild.Engine.Serialization;
using DigBuild.Engine.Storage;
using DigBuild.Engine.Worlds.Impl;
using DigBuild.Registries;

namespace DigBuild.Behaviors
{
    /// <summary>
    /// The contract for the item entity behavior.
    /// </summary>
    public interface IItemEntityBehavior
    {
        /// <summary>
        /// The item represented by the entity.
        /// </summary>
        ItemInstance Item { get; set; }
        /// <summary>
        /// The time the entity joined the world.
        /// </summary>
        ulong JoinWorldTime { get; set; }

        /// <summary>
        /// The item entity capability instance.
        /// </summary>
        IItemEntity? Capability { get; set; }
    }

    /// <summary>
    /// An item entity behavior. Contract: <see cref="IItemEntityBehavior"/>
    /// <para>
    /// Entities with this behavior will expire after 5 minutes.
    /// </para>
    /// <para>
    /// Provides the <see cref="ItemEntityModelData"/> class.
    /// </para>
    /// </summary>
    public sealed class ItemEntityBehavior : IEntityBehavior<IItemEntityBehavior>
    {
        public void Init(IItemEntityBehavior data)
        {
            data.Item ??= ItemInstance.Empty;
            data.Capability = new ItemEntity(data);
        }

        public void Build(EntityBehaviorBuilder<IItemEntityBehavior, IItemEntityBehavior> entity)
        {
            entity.Add(GameEntityAttributes.Item, (_, data, _) => data.Item);
            entity.Add(GameEntityCapabilities.ItemEntity, (_, data, _) => data.Capability);
            entity.Add(ModelData.EntityAttribute, GetModelData);
            entity.Subscribe(OnJoinedWorld);
        }

        private void OnJoinedWorld(BuiltInEntityEvent.JoinedWorld evt, IItemEntityBehavior data, Action next)
        {
            data.JoinWorldTime = evt.Entity.World.AbsoluteTime;
            evt.Entity.World.TickScheduler.After(5 * 60 * TickSource.TicksPerSecond).Tick += () =>
            {
                evt.Entity.World.RemoveEntity(evt.Entity.Id);
            };
            next();
        }

        private ModelData GetModelData(IReadOnlyEntityInstance instance, IItemEntityBehavior data, Func<ModelData> next)
        {
            var modelData = next();
            modelData.CreateOrExtend<ItemEntityModelData>(d =>
            {
                d.Item = data.Item;
                d.JoinWorldTime = data.JoinWorldTime;
                d.WorldTime = instance.World.AbsoluteTime;
            });
            return modelData;
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

    /// <summary>
    /// Data class fulfilling the contract of <see cref="IItemEntityBehavior"/>.
    /// </summary>
    public sealed class ItemEntityData : IData<ItemEntityData>, IChangeNotifier, IItemEntityBehavior
    {
        private ItemInstance _item = ItemInstance.Empty;

        public event Action? Changed;

        public ItemInstance Item
        {
            get => _item;
            set
            {
                _item = value;
                Changed?.Invoke();
            }
        }

        public ulong JoinWorldTime { get; set; }
        IItemEntity? IItemEntityBehavior.Capability { get; set; }

        public ItemEntityData Copy()
        {
            return new()
            {
                Item = Item,
                JoinWorldTime = JoinWorldTime
            };
        }

        /// <summary>
        /// The serdes.
        /// </summary>
        public static ISerdes<ItemEntityData> Serdes { get; } =
            new CompositeSerdes<ItemEntityData>()
            {
                {1u, d => d.Item, ItemInstance.Serdes},
                {2u, d => d.JoinWorldTime, UnmanagedSerdes<ulong>.NotNull}
            };
    }

    /// <summary>
    /// Item entity capability interface.
    /// </summary>
    public interface IItemEntity
    {
        /// <summary>
        /// The item instance.
        /// </summary>
        public ItemInstance Item { get; set; }
    }

    /// <summary>
    /// An item entity's model data.
    /// </summary>
    public sealed class ItemEntityModelData
    {
        /// <summary>
        /// The item.
        /// </summary>
        public ItemInstance Item { get; set; } = ItemInstance.Empty;
        /// <summary>
        /// The time the entity joined the world.
        /// </summary>
        public ulong JoinWorldTime { get; set; }
        /// <summary>
        /// The current world time.
        /// </summary>
        public ulong WorldTime { get; set; }
    }
}