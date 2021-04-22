using System;
using DigBuild.Blocks;
using DigBuild.Engine.Blocks;

namespace DigBuild.Content.Behaviors
{
    public sealed class NoPunchBehavior : IBlockBehavior
    {
        public void Build(BlockBehaviorBuilder<object, object> block)
        {
            block.Subscribe(OnPunch);
        }

        private BlockEvent.Punch.Result OnPunch(BlockEvent.Punch evt, object data, Func<BlockEvent.Punch.Result> next)
        {
            return BlockEvent.Punch.Result.Fail;
        }
    }
}