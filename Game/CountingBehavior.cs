using System;
using DigBuild.Blocks;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Items;
using DigBuild.Items;

namespace DigBuild
{
    public interface ICountingBehavior
    {
        int Number { get; set; }
    }

    public sealed class CountingBehavior : IBlockBehavior<ICountingBehavior>, IItemBehavior<ICountingBehavior>
    {
        public void Build(BlockBehaviorBuilder<ICountingBehavior> block)
        {
            block.Subscribe(OnActivate);
        }

        public void Build(ItemBehaviorBuilder<ICountingBehavior> item)
        {
            item.Subscribe(OnActivate);
        }

        private BlockEvent.Activate.Result OnActivate(IBlockContext context, ICountingBehavior data, BlockEvent.Activate evt, Func<BlockEvent.Activate.Result> next)
        {
            data.Number++;
            Console.WriteLine($"Counting! {data.Number}");
            return BlockEvent.Activate.Result.Success;
        }

        private ItemEvent.Activate.Result OnActivate(IPlayerItemContext context, ICountingBehavior data, ItemEvent.Activate evt, Func<ItemEvent.Activate.Result> next)
        {
            data.Number++;
            Console.WriteLine($"Counting! {data.Number}");
            return ItemEvent.Activate.Result.Success;
        }
    }

    public sealed class CountingData : ICountingBehavior
    {
        public int Number { get; set; }
    }
}