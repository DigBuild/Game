using DigBuild.Engine.Blocks;
using DigBuild.Engine.BuiltIn;
using DigBuild.Engine.Impl.Worlds;
using DigBuild.Engine.Items;
using DigBuild.Engine.Math;
using DigBuild.Engine.Registries;
using DigBuild.Engine.Worlds;
using DigBuild.Players;
using DigBuild.Worlds;

namespace DigBuild.Blocks
{
    public static class BlockEvent
    {
        public static void Register(ExtendedTypeRegistryBuilder<IBlockEvent, BlockEventInfo> registry)
        {
            registry.Register((Activate evt) => Activate.Result.Fail);
            registry.Register((Punch evt) =>
            {
                evt.Block.OnBreaking(evt.World, evt.Pos);
                evt.World.SetBlock(evt.Hit.BlockPos, null, false, true);
                return Punch.Result.Success;
            });
            registry.Register((NeighborChanged evt) => { });
            registry.Register((Placed evt) =>
            {
                evt.Block.OnJoinedWorld(evt.World, evt.Pos);
            });
            registry.Register((Breaking evt) =>
            {
                evt.Block.OnLeavingWorld(evt.World, evt.Pos);
            });
        }

        public sealed class Activate : BlockContext, IBlockEvent<Activate.Result>
        {
            public WorldRayCastContext.Hit Hit { get; }
            public IPlayer Player { get; }

            public Activate(IWorld world, BlockPos pos, Block block, WorldRayCastContext.Hit hit, IPlayer player) : base(world, pos, block)
            {
                Hit = hit;
                Player = player;
            }

            public enum Result
            {
                Success, Fail
            }
        }

        public sealed class Punch : BlockContext, IBlockEvent<Punch.Result>
        {
            public WorldRayCastContext.Hit Hit { get; }
            public IPlayer Player { get; }

            public Punch(IWorld world, BlockPos pos, Block block, WorldRayCastContext.Hit hit, IPlayer player) : base(world, pos, block)
            {
                Hit = hit;
                Player = player;
            }

            public enum Result
            {
                Success, Fail
            }
        }

        public sealed class NeighborChanged : BlockContext, IBlockEvent
        {
            public Direction Direction { get; }

            public NeighborChanged(IWorld world, BlockPos pos, Block block, Direction direction) : base(world, pos, block)
            {
                Direction = direction;
            }
        }

        public sealed class Placed : BlockContext, IBlockEvent
        {
            public ItemInstance Item { get; }
            public IPlayer Player { get; }

            public Placed(IWorld world, BlockPos pos, Block block, ItemInstance item, IPlayer player) :
                base(world, pos, block)
            {
                Item = item;
                Player = player;
            }
        }

        public sealed class Breaking : BlockContext, IBlockEvent
        {
            public Breaking(IWorld world, BlockPos pos, Block block) : base(world, pos, block)
            {
            }
        }
    }

    public static class BlockEventExtensions
    {
        public static void Subscribe<TReadOnlyData, TData>(
            this IBlockBehaviorBuilder<TReadOnlyData, TData> builder,
            BlockEventDelegate<TData, BlockEvent.Activate, BlockEvent.Activate.Result> onActivate
        )
            where TData : TReadOnlyData
        {
            builder.Subscribe(onActivate);
        }

        public static BlockEvent.Activate.Result OnActivate(
            this Block block, IWorld world, BlockPos pos,
            WorldRayCastContext.Hit hit, IPlayer player
        )
        {
            return block.Post<BlockEvent.Activate, BlockEvent.Activate.Result>(new BlockEvent.Activate(world, pos, block, hit, player));
        }

        public static void Subscribe<TReadOnlyData, TData>(
            this IBlockBehaviorBuilder<TReadOnlyData, TData> builder,
            BlockEventDelegate<TData, BlockEvent.Punch, BlockEvent.Punch.Result> onPunch
        )
            where TData : TReadOnlyData
        {
            builder.Subscribe(onPunch);
        }

        public static BlockEvent.Punch.Result OnPunch(
            this Block block, IWorld world, BlockPos pos,
            WorldRayCastContext.Hit hit, IPlayer player
        )
        {
            return block.Post<BlockEvent.Punch, BlockEvent.Punch.Result>(new BlockEvent.Punch(world, pos, block, hit, player));
        }
        
        public static void Subscribe<TReadOnlyData, TData>(
            this IBlockBehaviorBuilder<TReadOnlyData, TData> builder,
            BlockEventDelegate<TData, BlockEvent.NeighborChanged> onNeighborChanged
        )
            where TData : TReadOnlyData
        {
            builder.Subscribe(onNeighborChanged);
        }

        public static void OnNeighborChanged(this Block block, IWorld world, BlockPos pos, Direction direction)
        {
            block.Post(new BlockEvent.NeighborChanged(world, pos, block, direction));
        }
        
        public static void Subscribe<TReadOnlyData, TData>(
            this IBlockBehaviorBuilder<TReadOnlyData, TData> builder,
            BlockEventDelegate<TData, BlockEvent.Placed> onPlaced
        )
            where TData : TReadOnlyData
        {
            builder.Subscribe(onPlaced);
        }

        public static void OnPlaced(this Block block, IWorld world, BlockPos pos, ItemInstance item, IPlayer player)
        {
            block.Post(new BlockEvent.Placed(world, pos, block, item, player));
        }
        
        public static void Subscribe<TReadOnlyData, TData>(
            this IBlockBehaviorBuilder<TReadOnlyData, TData> builder,
            BlockEventDelegate<TData, BlockEvent.Breaking> onBreaking
        )
            where TData : TReadOnlyData
        {
            builder.Subscribe(onBreaking);
        }

        public static void OnBreaking(this Block block, IWorld world, BlockPos pos)
        {
            block.Post(new BlockEvent.Breaking(world, pos, block));
        }
    }
}