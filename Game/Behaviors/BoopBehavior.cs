using System;
using DigBuild.Blocks;
using DigBuild.Engine.Blocks;

namespace DigBuild.Behaviors
{
    public sealed class BoopBehavior : IBlockBehavior
    {
        public void Build(BlockBehaviorBuilder<object, object> block)
        {
            block.Subscribe(OnActivate);
        }

        private BlockEvent.Activate.Result OnActivate(IBlockContext context, object data, BlockEvent.Activate evt, Func<BlockEvent.Activate.Result> next)
        {
            context.World.TickScheduler.After(3 * 20).Enqueue(GameJobs.DelayedPrinter, $"{evt.Hit.BlockPos} @ {evt.Hit.Face}");
            return BlockEvent.Activate.Result.Success;
        }
    }
}