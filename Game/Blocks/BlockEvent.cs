using System;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Reg;

namespace DigBuild.Blocks
{
    public static class BlockEvent
    {
        public static void Register(ExtendedTypeRegistryBuilder<IBlockEvent, BlockEventInfo> registry)
        {
            registry.Register((IBlockContext context, Activate evt) => Activate.Result.Fail);
        }

        public sealed class Activate : IBlockEvent<IBlockContext, Activate.Result>
        {
            public readonly WorldRayCastContext.Hit Hit;

            public Activate(WorldRayCastContext.Hit hit)
            {
                Hit = hit;
            }

            public enum Result
            {
                Success, Fail
            }
        }
    }

    public static class BlockEventExtensions
    {
        public static void Subscribe<TData>(
            this IBlockBehaviorBuilder<TData> builder,
            BlockEventDelegate<IBlockContext, TData, BlockEvent.Activate, BlockEvent.Activate.Result> onActivate
        )
        {
            builder.Subscribe(onActivate);
        }

        public static BlockEvent.Activate.Result OnActivate(this IBlock block, IBlockContext context, BlockEvent.Activate evt)
        {
            return block.Post<IBlockContext, BlockEvent.Activate, BlockEvent.Activate.Result>(context, evt);
        }
    }
}