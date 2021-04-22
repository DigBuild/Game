using System;
using DigBuild.Blocks;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Impl.Worlds;
using DigBuild.Engine.Items;
using DigBuild.Items;

namespace DigBuild.Behaviors
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

            if (!world.SetBlock(pos, block, true, false))
                return ItemEvent.Activate.Result.Fail;

            block.OnPlaced(world, pos);
            evt.Item.Count--;
            return ItemEvent.Activate.Result.Success;
        }
    }
}