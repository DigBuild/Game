﻿using System;
using DigBuild.Blocks;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Entities;
using DigBuild.Engine.Math;
using DigBuild.Engine.Ticking;
using DigBuild.Engine.Voxel;
using DigBuild.Engine.Worldgen;

namespace DigBuild.Voxel
{
    public sealed class World : WorldBase
    {
        public ChunkManager ChunkManager { get; }

        public event Action<EntityInstance>? EntityAdded;
        public event Action<Guid>? EntityRemoved; 

        public World(WorldGenerator generator, Scheduler tickScheduler)
        {
            ChunkManager = new ChunkManager(generator);
            TickScheduler = tickScheduler;
        }

        public override Scheduler TickScheduler { get; }

        public override IChunk? GetChunk(ChunkPos pos, bool load = true)
        {
            return ChunkManager.Get(pos, load);
        }
        
        public override void OnBlockChanged(BlockPos pos)
        {
            foreach (var face in BlockFaces.All)
            {
                var offset = pos.Offset(face);
                var block = this.GetBlock(offset);
                block?.OnNeighborChanged(new BlockContext(this, offset, block), new BlockEvent.NeighborChanged(face.GetOpposite()));
                ChunkManager.OnBlockChanged(pos);
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