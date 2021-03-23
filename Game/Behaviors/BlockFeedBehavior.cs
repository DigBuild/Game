using System;
using DigBuild.Blocks;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Worlds;

namespace DigBuild.Behaviors
{
    public interface IBlockFeedBehavior
    {
        public ulong LastFed { get; set; }
    }

    public sealed class BlockFeedBehavior : IBlockBehavior<IBlockFeedBehavior>
    {
        private const ulong FeedDelay = 20 * 5;

        public void Build(BlockBehaviorBuilder<IBlockFeedBehavior> block)
        {
            block.Subscribe(OnPlaced);
            block.Subscribe(OnActivate);
        }

        private BlockEvent.Activate.Result OnActivate(IBlockContext context, IBlockFeedBehavior data, BlockEvent.Activate evt, Func<BlockEvent.Activate.Result> next)
        {
            DoTheThing(context, data);
            return BlockEvent.Activate.Result.Success;
        }

        private void OnPlaced(IBlockContext context, IBlockFeedBehavior data, BlockEvent.Placed evt, Action next)
        {
            DoTheThing(context, data);
        }

        private void DoTheThing(IBlockContext context, IBlockFeedBehavior data)
        {
            data.LastFed = context.World.AbsoluteTime;
            context.World.TickScheduler.After(FeedDelay).Tick += () => OnTimeElapsed(context, data);
        }

        private void OnTimeElapsed(IBlockContext context, IBlockFeedBehavior data)
        {
            if (context.World.GetBlock(context.Pos) != context.Block)
                return;
            
            if (context.World.AbsoluteTime - data.LastFed >= FeedDelay)
                context.World.SetBlock(context.Pos, null);
        }
    }

    public sealed class BlockFeedData : IBlockFeedBehavior
    {
        public ulong LastFed { get; set; }
    }
}