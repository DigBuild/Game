using System;
using DigBuild.Blocks;
using DigBuild.Engine.Entities;
using DigBuild.Engine.Impl.Worlds;
using DigBuild.Engine.Math;
using DigBuild.Engine.Ticking;

namespace DigBuild.Worlds
{
    public sealed class World : WorldBase
    {
        public const float GravityValue = 2.5f * TickSource.TickDurationSeconds;
        public const ulong DayDuration = 1000; // Ticks
        
        private ulong _absoluteTime;

        public override ulong AbsoluteTime => _absoluteTime;
        public override float Gravity => GravityValue;
        public override Scheduler TickScheduler { get; }

        public event Action<EntityInstance>? EntityAdded;
        public event Action<Guid>? EntityRemoved;
        public event Action<BlockPos>? BlockChanged;

        public World(IStableTickSource tickSource, IChunkProvider generator, Func<RegionPos, IRegionStorage> storageProvider) :
            base(tickSource, generator, storageProvider)
        {
            TickScheduler = new Scheduler(tickSource);
            tickSource.Tick += () =>
            {
                _absoluteTime++;
            };
        }

        public override void OnBlockChanged(BlockPos pos)
        {
            foreach (var face in Directions.All)
            {
                var offset = pos.Offset(face);
                var block = this.GetBlock(offset);
                block?.OnNeighborChanged(this, offset, face.GetOpposite());
            }
            BlockChanged?.Invoke(pos);
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