using System;
using System.Numerics;
using DigBuild.Engine.BuiltIn;
using DigBuild.Engine.Entities;
using DigBuild.Engine.Impl.Worlds;
using DigBuild.Engine.Math;
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
        public AABB Bounds { get; set; } = AABB.FullBlock - (Vector3.One / 2);

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

            var pos = entity.Get(EntityAttributes.Position);

            foreach (var itemEntity in entity.World.GetEntities(GameEntities.Item))
            {
                var itemPos = itemEntity.Get(EntityAttributes.Position);
                var relPos = itemPos - pos;

                if (!Bounds.Contains(relPos))
                    continue;

                var item = itemEntity.Get(EntityAttributes.Item)!;
                    
                var t = data.PickupTarget.BeginTransaction();
                if (t.Insert(item).Count == 0)
                {
                    t.Commit();
                    itemEntity.Remove();
                }
            }
            
            entity.World.TickScheduler.After(1).Tick += () => Update(entity, data);
        }
    }
}