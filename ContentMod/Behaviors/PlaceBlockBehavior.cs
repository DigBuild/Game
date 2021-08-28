using System;
using System.Numerics;
using DigBuild.Blocks;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Items;
using DigBuild.Engine.Worlds.Impl;
using DigBuild.Items;
using DigBuild.Registries;

namespace DigBuild.Content.Behaviors
{
    public sealed class PlaceBlockBehavior : IItemBehavior
    {
        private readonly Func<Block> _blockSupplier;

        public PlaceBlockBehavior(Func<Block> blockSupplier)
        {
            _blockSupplier = blockSupplier;
        }

        public void Build(ItemBehaviorBuilder<object, object> item)
        {
            item.Subscribe(OnActivate);
        }

        private ItemEvent.Activate.Result OnActivate(ItemEvent.Activate evt, object data, Func<ItemEvent.Activate.Result> next)
        {
            if (evt.Hit == null)
                return next();

            var pos = evt.Hit.BlockPos.Offset(evt.Hit.Face);
            var block = _blockSupplier();
            var world = evt.Player.Entity.World;

            var bounds = block.Get(world, pos, BlockAttributes.Bounds);
            if (bounds.HasValue)
            {
                var blockAABB = bounds.Value + (Vector3)pos;

                foreach (var entity in world.GetEntities())
                {
                    var entityBounds = entity.Get(EntityAttributes.Bounds);
                    var entityPos = entity.Get(EntityAttributes.Position);
                    if (entityBounds.HasValue && (entityBounds.Value + entityPos).Intersects(blockAABB))
                        return next();
                }
            }

            if (!world.SetBlock(pos, block, true, false))
                return ItemEvent.Activate.Result.Fail;

            block.OnPlaced(world, pos, evt.Item, evt.Player);
            evt.Item.Count--;
            return ItemEvent.Activate.Result.Success;
        }
    }
}