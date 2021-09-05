using System;
using System.Numerics;
using DigBuild.Engine.BuiltIn;
using DigBuild.Engine.Entities;
using DigBuild.Engine.Items.Inventories;
using DigBuild.Engine.Math;
using DigBuild.Engine.Worlds.Impl;
using DigBuild.Items;
using DigBuild.Registries;

namespace DigBuild.Behaviors
{
    /// <summary>
    /// The contract for the item pickup behavior.
    /// </summary>
    public interface IItemPickup
    {
        /// <summary>
        /// Whether the entity is in-world or not.
        /// </summary>
        bool InWorld { get; set; }

        /// <summary>
        /// The target inventory for picked up items.
        /// </summary>
        IInventory PickupTarget { get; }
    }

    /// <summary>
    /// An item pickup behavior for entities. Contract: <see cref="IItemPickup"/>
    /// <para>
    /// Attracts items within the <see cref="AttractionBounds"/> and tries to put
    /// them into the inventory when within the <see cref="PickupBounds"/>.
    /// </para>
    /// </summary>
    public sealed class ItemPickupBehavior : IEntityBehavior<IItemPickup>
    {
        /// <summary>
        /// The attraction bounding box.
        /// </summary>
        public AABB AttractionBounds { get; set; } = AABB.FullBlock - (Vector3.One / 2);

        /// <summary>
        /// The pickup bounding box.
        /// </summary>
        public AABB PickupBounds { get; set; } = AABB.FullBlock - (Vector3.One / 2);

        public void Build(EntityBehaviorBuilder<IItemPickup, IItemPickup> entity)
        {
            entity.Subscribe(OnJoinedWorld);
            entity.Subscribe(OnLeavingWorld);
        }

        private void OnJoinedWorld(BuiltInEntityEvent.JoinedWorld evt, IItemPickup data, Action next)
        {
            data.InWorld = true;

            evt.Entity.World.TickScheduler.After(1).Tick += () => Update(evt.Entity, data);

            next();
        }

        private void OnLeavingWorld(BuiltInEntityEvent.LeavingWorld evt, IItemPickup data, Action next)
        {
            data.InWorld = false;
        }

        private void Update(EntityInstance entity, IItemPickup data)
        {
            if (!data.InWorld) return;

            var pos = entity.Get(GameEntityAttributes.Position) + PickupBounds.Center;

            foreach (var itemEntity in entity.World.GetEntities(GameEntities.Item))
            {
                var itemPos = itemEntity.Get(GameEntityAttributes.Position);
                var relPos = itemPos - pos;

                if (!AttractionBounds.Contains(relPos))
                    continue;

                if (PickupBounds.Contains(relPos))
                {
                    var item = itemEntity.Get(GameEntityAttributes.Item)!.Copy();
                    
                    var t = data.PickupTarget.BeginTransaction();
                    if (t.Insert(item).Count == 0)
                    {
                        t.Commit();
                        itemEntity.Remove();
                    }
                }
                else
                {
                    var distance = relPos.Length();
                    var unitMotion = -relPos / distance;
                    var motion = unitMotion * MathF.Exp(-distance * 0.75f);

                    itemEntity.Get(GameEntityCapabilities.PhysicsEntity)!.Velocity += motion;
                }
            }
            
            entity.World.TickScheduler.After(1).Tick += () => Update(entity, data);
        }
    }
}