using System;
using System.Collections.Generic;
using DigBuild.Blocks;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Impl.Worlds;
using DigBuild.Engine.Items;
using DigBuild.Engine.Math;
using DigBuild.Items;

namespace DigBuild.Behaviors
{
    public sealed class PlaceMultiblockBehavior : IItemBehavior
    {
        private readonly Func<Dictionary<Vector3I, Block>> _blockSupplier;

        public PlaceMultiblockBehavior(Func<Dictionary<Vector3I, Block>> blockSupplier)
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

            var origin = evt.Hit.BlockPos.Offset(evt.Hit.Face);
            var blocks = _blockSupplier();
            var world = evt.Player.Entity.World;

            var master = (BlockPos?) null;

            foreach (var (offset, block) in blocks)
            {
                var pos = origin + offset;
                
                if (!world.SetBlock(pos, block, true, false))
                    return ItemEvent.Activate.Result.Fail;

                if (master == null)
                {
                    master = pos;
                    MultiblockBehavior.InitMaster(world, pos, new HashSet<Vector3I>(blocks.Keys));
                }
                else
                {
                    MultiblockBehavior.InitSlave(world, pos, master.Value);
                }
            }

            foreach (var (offset, block) in blocks)
            {
                var pos = origin + offset;
                block.OnPlaced(world, pos);
            }
            
            evt.Item.Count--;
            return ItemEvent.Activate.Result.Success;
        }
    }
}