using System;
using DigBuild.Blocks;
using DigBuild.Engine.Entities;
using DigBuild.Engine.Math;
using DigBuild.Engine.Ticking;
using DigBuild.Engine.Worldgen;
using DigBuild.Engine.Worlds;

namespace DigBuild.Worlds
{
    public sealed class World : WorldBase
    {
        private const float GravityValue = 2.5f * TickSource.TickDurationSeconds;

        public event Action<EntityInstance>? EntityAdded;
        public event Action<Guid>? EntityRemoved;

        public event Action<BlockPos>? BlockChanged;

        public World(WorldGenerator generator, IStableTickSource tickSource, Func<ChunkPos> generationOriginGetter)
        {
            ChunkManager = new ChunkManager(generator, generationOriginGetter);
            TickScheduler = new Scheduler(tickSource);
            tickSource.Tick += () =>
            {
                _absoluteTime++;
                ChunkManager.Update();
            };
        }

        private ulong _absoluteTime;
        public override ulong AbsoluteTime => _absoluteTime;
        public override float Gravity => GravityValue;

        public override ChunkManager ChunkManager { get; }

        public override Scheduler TickScheduler { get; }

        public override IChunk? GetChunk(ChunkPos pos, bool load = true)
        {
            return ChunkManager.Get(pos, load);
        }
        
        public override void OnBlockChanged(BlockPos pos)
        {
            foreach (var face in Directions.All)
            {
                var offset = pos.Offset(face);
                var block = this.GetBlock(offset);
                block?.OnNeighborChanged(this, offset, face.GetOpposite());
                ChunkManager.OnBlockChanged(pos);
                BlockChanged?.Invoke(pos);
            }
        }

        public override void OnEntityAdded(EntityInstance entity)
        {
            EntityAdded?.Invoke(entity);
        }

        public override void OnEntityRemoved(Guid guid)
        {
            EntityRemoved?.Invoke(guid);
        }
    }
}