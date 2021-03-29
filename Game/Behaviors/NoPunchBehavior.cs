using System;
using DigBuild.Blocks;
using DigBuild.Engine.Blocks;

namespace DigBuild.Behaviors
{
    public sealed class NoPunchBehavior : IBlockBehavior
    {
        public void Build(BlockBehaviorBuilder<object, object> block)
        {
            block.Subscribe(OnPunch);
        }

        private BlockEvent.Punch.Result OnPunch(IBlockContext context, object data, BlockEvent.Punch evt, Func<BlockEvent.Punch.Result> next)
        {
            return BlockEvent.Punch.Result.Fail;
        }
    }
}