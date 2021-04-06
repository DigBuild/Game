using System;
using DigBuild.Blocks;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Items;
using DigBuild.Engine.Worlds;
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

        private ItemEvent.Activate.Result OnActivate(IItemContext context, object data, ItemEvent.Activate evt, Func<ItemEvent.Activate.Result> next)
        {
            if (evt.Hit == null)
                return next();

            var pos = evt.Hit.BlockPos.Offset(evt.Hit.Face);
            var block = _blockSupplier();
            var world = evt.Player.Entity.World;

            if (!world.SetBlock(pos, block, true, false))
                return ItemEvent.Activate.Result.Fail;

            block.OnPlaced(new BlockContext(world, pos, block), new BlockEvent.Placed());
            context.Instance.Count--;
            return ItemEvent.Activate.Result.Success;
        }
    }
}