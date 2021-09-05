using System;
using DigBuild.Blocks;
using DigBuild.Engine.BuiltIn;
using DigBuild.Engine.Entities;
using DigBuild.Engine.Events;
using DigBuild.Engine.Math;
using DigBuild.Engine.Ticking;
using DigBuild.Engine.Worlds;
using DigBuild.Engine.Worlds.Impl;

namespace DigBuild.Worlds
{
    /// <summary>
    /// The game's world.
    /// </summary>
    public sealed class World : WorldBase
    {
        public const float GravityValue = 2.5f * TickSource.TickDurationSeconds;
        public const ulong DayDuration = 10000; // Ticks

        private readonly EventBus _eventBus;
        private readonly Action<ChunkPos> _notifyChunkReRender;
        
        private ulong _absoluteTime;

        public override ulong AbsoluteTime => _absoluteTime;
        public override float Gravity => GravityValue;
        public override Scheduler TickScheduler { get; }
        
        public World(
            IStableTickSource tickSource,
            IChunkProvider generator,
            Func<IWorld, RegionPos, IRegionStorageHandler> storageProvider,
            EventBus eventBus,
            Action<ChunkPos> notifyChunkReRender
        ) : base(tickSource, generator, storageProvider, eventBus)
        {
            _eventBus = eventBus;
            _notifyChunkReRender = notifyChunkReRender;
            TickScheduler = new Scheduler(tickSource);
            tickSource.Tick += () =>
            {
                _absoluteTime++;
            };

            _eventBus.Subscribe<BuiltInChunkEvent.Loaded>(evt =>
            {
                foreach (var direction in Directions.Horizontal)
                    MarkChunkForReRender(evt.Chunk.Position.Offset(direction));
            });
        }

        public override void OnBlockChanged(BlockPos pos)
        {
            foreach (var face in Directions.All)
            {
                var offset = pos.Offset(face);
                var block = this.GetBlock(offset);
                block?.OnNeighborChanged(this, offset, face.GetOpposite());
            }
        }

        public override void OnEntityAdded(EntityInstance entity)
        {
            _eventBus.Post(new BuiltInEntityEvent.JoinedWorld(entity));
        }

        public override void OnEntityRemoving(EntityInstance entity)
        {
            _eventBus.Post(new BuiltInEntityEvent.LeavingWorld(entity));
        }

        public override void MarkChunkForReRender(ChunkPos pos)
        {
            _notifyChunkReRender(pos);
        }

        public override void MarkBlockForReRender(BlockPos pos)
        {
            var (chunkPos, (subX, _, subZ)) = pos;
            MarkChunkForReRender(chunkPos);

            if (subX == 0)
                MarkChunkForReRender(chunkPos.Offset(Direction.NegX));
            else if (subX == WorldDimensions.ChunkWidth - 1)
                MarkChunkForReRender(chunkPos.Offset(Direction.PosX));

            if (subZ == 0)
                MarkChunkForReRender(chunkPos.Offset(Direction.NegZ));
            else if (subZ == WorldDimensions.ChunkWidth - 1)
                MarkChunkForReRender(chunkPos.Offset(Direction.PosZ));
        }
    }
}