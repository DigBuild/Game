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
    public interface IItemPickup
    {
        bool InWorld { get; set; }

        IInventory PickupTarget { get; }
    }

    public sealed class ItemPickupBehavior : IEntityBehavior<IItemPickup>
    {
        public AABB AttractionBounds { get; set; } = AABB.FullBlock - (Vector3.One / 2);
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

            var pos = entity.Get(EntityAttributes.Position) + PickupBounds.Center;

            foreach (var itemEntity in entity.World.GetEntities(GameEntities.Item))
            {
                var itemPos = itemEntity.Get(EntityAttributes.Position);
                var relPos = itemPos - pos;

                if (!AttractionBounds.Contains(relPos))
                    continue;

                if (PickupBounds.Contains(relPos))
                {
                    var item = itemEntity.Get(EntityAttributes.Item)!;
                    
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

                    itemEntity.Get(EntityCapabilities.PhysicalEntity)!.Velocity += motion;
                }
            }
            
            entity.World.TickScheduler.After(1).Tick += () => Update(entity, data);
        }
    }
}