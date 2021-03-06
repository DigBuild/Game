using System;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Voxel;

namespace DigBuild.Blocks
{
    public interface ICountingBehavior
    {
        int Number { get; set; }
    }

    public sealed class CountingBehavior : IBlockBehavior<ICountingBehavior>
    {
        public void Build(BlockBehaviorBuilder<ICountingBehavior> block)
        {
            block.Subscribe(OnActivate);
        }

        private BlockEvent.Activate.Result OnActivate(IBlockContext context, ICountingBehavior data, BlockEvent.Activate evt, Func<BlockEvent.Activate.Result> next)
        {
            data.Number++;
            Console.WriteLine($"Counting! {data.Number}");
            return BlockEvent.Activate.Result.Success;
        }
    }
}