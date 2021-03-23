using DigBuild.Engine.Blocks;
using DigBuild.Engine.BuiltIn;
using DigBuild.Engine.Math;
using DigBuild.Engine.Registries;
using DigBuild.Engine.Worlds;
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
                context.Block.OnBreaking(context, new Breaking());
                context.World.SetBlock(evt.Hit.BlockPos, null, false, true);
                return Punch.Result.Success;
            });
            registry.Register((IBlockContext context, NeighborChanged evt) => { });
            registry.Register((IBlockContext context, Placed evt) =>
            {
                context.Block.OnJoinedWorld(context, new BuiltInBlockEvent.JoinedWorld());
            });
            registry.Register((IBlockContext context, Breaking evt) =>
            {
                context.Block.OnLeavingWorld(context, new BuiltInBlockEvent.LeavingWorld());
            });
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

        public sealed class Breaking : IBlockEvent<IBlockContext>
        {
            public Breaking()
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
            BlockEventDelegate<IBlockContext, TData, BlockEvent.Breaking> onBreaking
        )
        {
            builder.Subscribe(onBreaking);
        }

        public static void OnBreaking(this IBlock block, IBlockContext context, BlockEvent.Breaking evt)
        {
            block.Post(context, evt);
        }
    }
}