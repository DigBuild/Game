using DigBuild.Engine.Blocks;
using DigBuild.Engine.BuiltIn;
using DigBuild.Engine.Items;
using DigBuild.Engine.Math;
using DigBuild.Engine.Registries;
using DigBuild.Engine.Worlds;
using DigBuild.Engine.Worlds.Impl;
using DigBuild.Players;
using DigBuild.Worlds;

namespace DigBuild.Blocks
{
    /// <summary>
    /// The game's block events.
    /// </summary>
    public static class BlockEvent
    {
        internal static void Register(TypeRegistryBuilder<IBlockEvent, BlockEventInfo> registry)
        {
            registry.Register((Activate _) => Activate.Result.Fail);
            registry.Register((Punch evt) =>
            {
                evt.Block.OnBreaking(evt.World, evt.Pos);
                evt.World.SetBlock(evt.Hit.BlockPos, null, false, true);
                return Punch.Result.Success;
            });
            registry.Register((NeighborChanged _) => { });
            registry.Register((Placed evt) =>
            {
                evt.Block.OnJoinedWorld(evt.World, evt.Pos);
            });
            registry.Register((Breaking evt) =>
            {
                evt.Block.OnLeavingWorld(evt.World, evt.Pos);
            });
        }

        /// <summary>
        /// Fired when a block is left-clicked.
        /// </summary>
        public sealed class Activate : BlockContext, IBlockEvent<Activate.Result>
        {
            /// <summary>
            /// The raycast hit on the block.
            /// </summary>
            public WorldRayCastContext.Hit Hit { get; }
            /// <summary>
            /// The player that activated the block.
            /// </summary>
            public IPlayer Player { get; }

            public Activate(IWorld world, BlockPos pos, Block block, WorldRayCastContext.Hit hit, IPlayer player) : base(world, pos, block)
            {
                Hit = hit;
                Player = player;
            }

            /// <summary>
            /// The result state of the event.
            /// </summary>
            public enum Result
            {
                Success, Fail
            }
        }

        /// <summary>
        /// Fired when a block is right-clicked.
        /// </summary>
        public sealed class Punch : BlockContext, IBlockEvent<Punch.Result>
        {
            /// <summary>
            /// The raycast hit on the block.
            /// </summary>
            public WorldRayCastContext.Hit Hit { get; }
            /// <summary>
            /// The player that punched the block.
            /// </summary>
            public IPlayer Player { get; }

            public Punch(IWorld world, BlockPos pos, Block block, WorldRayCastContext.Hit hit, IPlayer player) : base(world, pos, block)
            {
                Hit = hit;
                Player = player;
            }
            
            /// <summary>
            /// The result state of the event.
            /// </summary>
            public enum Result
            {
                Success, Fail
            }
        }

        /// <summary>
        /// Fired when a neighboring block changes.
        /// </summary>
        public sealed class NeighborChanged : BlockContext, IBlockEvent
        {
            /// <summary>
            /// The direction of the neighbor.
            /// </summary>
            public Direction Direction { get; }

            public NeighborChanged(IWorld world, BlockPos pos, Block block, Direction direction) : base(world, pos, block)
            {
                Direction = direction;
            }
        }

        /// <summary>
        /// Fired when the block is placed.
        /// </summary>
        public sealed class Placed : BlockContext, IBlockEvent
        {
            /// <summary>
            /// The item instance used to place the block.
            /// </summary>
            public ItemInstance Item { get; }
            /// <summary>
            /// The player that placed the block.
            /// </summary>
            public IPlayer Player { get; }

            public Placed(IWorld world, BlockPos pos, Block block, ItemInstance item, IPlayer player) :
                base(world, pos, block)
            {
                Item = item;
                Player = player;
            }
        }

        /// <summary>
        /// Fired when a block is about to be broken.
        /// </summary>
        public sealed class Breaking : BlockContext, IBlockEvent
        {
            public Breaking(IWorld world, BlockPos pos, Block block) : base(world, pos, block)
            {
            }
        }
    }
    
    /// <summary>
    /// Registration/subscription extensions for block events.
    /// </summary>
    public static class BlockEventExtensions
    {
        /// <summary>
        /// Subscribes to the activated event.
        /// </summary>
        /// <typeparam name="TReadOnlyData">The read-only data type</typeparam>
        /// <typeparam name="TData">The read-write data type</typeparam>
        /// <param name="builder">The builder</param>
        /// <param name="onActivate">The handler</param>
        public static void Subscribe<TReadOnlyData, TData>(
            this IBlockBehaviorBuilder<TReadOnlyData, TData> builder,
            BlockEventDelegate<TData, BlockEvent.Activate, BlockEvent.Activate.Result> onActivate
        )
            where TData : TReadOnlyData
        {
            builder.Subscribe(onActivate);
        }
        
        /// <summary>
        /// Fires the activated event.
        /// </summary>
        /// <param name="block">The block</param>
        /// <param name="world">The world</param>
        /// <param name="pos">The position</param>
        /// <param name="hit">The raycast hit</param>
        /// <param name="player">The player</param>
        public static BlockEvent.Activate.Result OnActivate(
            this Block block, IWorld world, BlockPos pos,
            WorldRayCastContext.Hit hit, IPlayer player
        )
        {
            return block.Post<BlockEvent.Activate, BlockEvent.Activate.Result>(new BlockEvent.Activate(world, pos, block, hit, player));
        }
        
        /// <summary>
        /// Subscribes to the punched event.
        /// </summary>
        /// <typeparam name="TReadOnlyData">The read-only data type</typeparam>
        /// <typeparam name="TData">The read-write data type</typeparam>
        /// <param name="builder">The builder</param>
        /// <param name="onPunch">The handler</param>
        public static void Subscribe<TReadOnlyData, TData>(
            this IBlockBehaviorBuilder<TReadOnlyData, TData> builder,
            BlockEventDelegate<TData, BlockEvent.Punch, BlockEvent.Punch.Result> onPunch
        )
            where TData : TReadOnlyData
        {
            builder.Subscribe(onPunch);
        }
        
        /// <summary>
        /// Fires the punched event.
        /// </summary>
        /// <param name="block">The block</param>
        /// <param name="world">The world</param>
        /// <param name="pos">The position</param>
        /// <param name="hit">The raycast hit</param>
        /// <param name="player">The player</param>
        public static BlockEvent.Punch.Result OnPunch(
            this Block block, IWorld world, BlockPos pos,
            WorldRayCastContext.Hit hit, IPlayer player
        )
        {
            return block.Post<BlockEvent.Punch, BlockEvent.Punch.Result>(new BlockEvent.Punch(world, pos, block, hit, player));
        }
        
        /// <summary>
        /// Subscribes to the neighbor changed event.
        /// </summary>
        /// <typeparam name="TReadOnlyData">The read-only data type</typeparam>
        /// <typeparam name="TData">The read-write data type</typeparam>
        /// <param name="builder">The builder</param>
        /// <param name="onNeighborChanged">The handler</param>
        public static void Subscribe<TReadOnlyData, TData>(
            this IBlockBehaviorBuilder<TReadOnlyData, TData> builder,
            BlockEventDelegate<TData, BlockEvent.NeighborChanged> onNeighborChanged
        )
            where TData : TReadOnlyData
        {
            builder.Subscribe(onNeighborChanged);
        }

        /// <summary>
        /// Fires the neighbor changed event.
        /// </summary>
        /// <param name="block">The block</param>
        /// <param name="world">The world</param>
        /// <param name="pos">The position</param>
        /// <param name="direction">The direction the neighbor is in</param>
        public static void OnNeighborChanged(this Block block, IWorld world, BlockPos pos, Direction direction)
        {
            block.Post(new BlockEvent.NeighborChanged(world, pos, block, direction));
        }
        
        /// <summary>
        /// Subscribes to the placed event.
        /// </summary>
        /// <typeparam name="TReadOnlyData">The read-only data type</typeparam>
        /// <typeparam name="TData">The read-write data type</typeparam>
        /// <param name="builder">The builder</param>
        /// <param name="onPlaced">The handler</param>
        public static void Subscribe<TReadOnlyData, TData>(
            this IBlockBehaviorBuilder<TReadOnlyData, TData> builder,
            BlockEventDelegate<TData, BlockEvent.Placed> onPlaced
        )
            where TData : TReadOnlyData
        {
            builder.Subscribe(onPlaced);
        }

        /// <summary>
        /// Fires the placed event.
        /// </summary>
        /// <param name="block">The block</param>
        /// <param name="world">The world</param>
        /// <param name="pos">The position</param>
        /// <param name="item">The item used to place the block</param>
        /// <param name="player">The player</param>
        public static void OnPlaced(this Block block, IWorld world, BlockPos pos, ItemInstance item, IPlayer player)
        {
            block.Post(new BlockEvent.Placed(world, pos, block, item, player));
        }
        
        /// <summary>
        /// Subscribes to the breaking event.
        /// </summary>
        /// <typeparam name="TReadOnlyData">The read-only data type</typeparam>
        /// <typeparam name="TData">The read-write data type</typeparam>
        /// <param name="builder">The builder</param>
        /// <param name="onBreaking">The handler</param>
        public static void Subscribe<TReadOnlyData, TData>(
            this IBlockBehaviorBuilder<TReadOnlyData, TData> builder,
            BlockEventDelegate<TData, BlockEvent.Breaking> onBreaking
        )
            where TData : TReadOnlyData
        {
            builder.Subscribe(onBreaking);
        }
        
        /// <summary>
        /// Fires the breaking event.
        /// </summary>
        /// <param name="block">The block</param>
        /// <param name="world">The world</param>
        /// <param name="pos">The position</param>
        public static void OnBreaking(this Block block, IWorld world, BlockPos pos)
        {
            block.Post(new BlockEvent.Breaking(world, pos, block));
        }
    }
}