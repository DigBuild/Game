using DigBuild.Engine.Blocks;
using DigBuild.Engine.Math;
using DigBuild.Engine.Reg;
using DigBuild.Engine.Voxel;
using DigBuild.Voxel;

namespace DigBuild.Blocks
{
    public static class BlockEvent
    {
        public static void Register(ExtendedTypeRegistryBuilder<IBlockEvent, BlockEventInfo> registry)
        {
            registry.Register((IBlockContext context, Activate evt) => Activate.Result.Fail);
            registry.Register((IBlockContext context, Punch evt) =>
            {
                context.Block.OnBroken(context, new Broken());
                context.World.SetBlock(evt.Hit.BlockPos, null);
                return Punch.Result.Success;
            });
            registry.Register((IBlockContext context, NeighborChanged evt) => { });
            registry.Register((IBlockContext context, Placed evt) => { });
            registry.Register((IBlockContext context, Broken evt) => { });
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

        public sealed class Punch : IBlockEvent<IBlockContext, Punch.Result>
        {
            public readonly WorldRayCastContext.Hit Hit;

            public Punch(WorldRayCastContext.Hit hit)
            {
                Hit = hit;
            }

            public enum Result
            {
                Success, Fail
            }
        }

        public sealed class NeighborChanged : IBlockEvent<IBlockContext>
        {
            public readonly BlockFace Direction;

            public NeighborChanged(BlockFace direction)
            {
                Direction = direction;
            }
        }

        public sealed class Placed : IBlockEvent<IBlockContext>
        {
            public Placed()
            {
            }
        }

        public sealed class Broken : IBlockEvent<IBlockContext>
        {
            public Broken()
            {
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

        public static void Subscribe<TData>(
            this IBlockBehaviorBuilder<TData> builder,
            BlockEventDelegate<IBlockContext, TData, BlockEvent.Punch, BlockEvent.Punch.Result> onPunch
        )
        {
            builder.Subscribe(onPunch);
        }

        public static BlockEvent.Punch.Result OnPunch(this IBlock block, IBlockContext context, BlockEvent.Punch evt)
        {
            return block.Post<IBlockContext, BlockEvent.Punch, BlockEvent.Punch.Result>(context, evt);
        }
        
        public static void Subscribe<TData>(
            this IBlockBehaviorBuilder<TData> builder,
            BlockEventDelegate<IBlockContext, TData, BlockEvent.NeighborChanged> onNeighborChanged
        )
        {
            builder.Subscribe(onNeighborChanged);
        }

        public static void OnNeighborChanged(this IBlock block, IBlockContext context, BlockEvent.NeighborChanged evt)
        {
            block.Post(context, evt);
        }
        
        public static void Subscribe<TData>(
            this IBlockBehaviorBuilder<TData> builder,
            BlockEventDelegate<IBlockContext, TData, BlockEvent.Placed> onPlaced
        )
        {
            builder.Subscribe(onPlaced);
        }

        public static void OnPlaced(this IBlock block, IBlockContext context, BlockEvent.Placed evt)
        {
            block.Post(context, evt);
        }
        
        public static void Subscribe<TData>(
            this IBlockBehaviorBuilder<TData> builder,
            BlockEventDelegate<IBlockContext, TData, BlockEvent.Broken> onBroken
        )
        {
            builder.Subscribe(onBroken);
        }

        public static void OnBroken(this IBlock block, IBlockContext context, BlockEvent.Broken evt)
        {
            block.Post(context, evt);
        }
    }
}