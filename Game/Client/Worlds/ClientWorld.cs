using System;
using DigBuild.Engine.Entities;
using DigBuild.Engine.Impl.Worlds;
using DigBuild.Engine.Math;
using DigBuild.Engine.Ticking;
using DigBuild.Worlds;

namespace DigBuild.Client.Worlds
{
    public sealed class ClientWorld : WorldBase
    {
        private readonly NetworkChunkProvider _chunkProvider;
        
        private ulong _absoluteTime;

        public override ulong AbsoluteTime => _absoluteTime;
        public override float Gravity => World.GravityValue;
        public override Scheduler TickScheduler { get; }

        public event Action<EntityInstance>? EntityAdded;
        public event Action<Guid>? EntityRemoved;
        public event Action<BlockPos>? BlockChanged;

        public ClientWorld(IStableTickSource tickSource) :
            this(tickSource, new NetworkChunkProvider(tickSource))
        {
        }

        private ClientWorld(IStableTickSource tickSource, NetworkChunkProvider chunkProvider) :
            base(tickSource, chunkProvider, _ => NullRegionStorage.Instance)
        {
            _chunkProvider = chunkProvider;

            TickScheduler = new Scheduler(tickSource);
            tickSource.Tick += () =>
            {
                _absoluteTime++;
            };
        }
        
        public void Add(Chunk chunk)
        {
            _chunkProvider.Add(chunk);
        }
        
        public override void OnBlockChanged(BlockPos pos)
        {
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