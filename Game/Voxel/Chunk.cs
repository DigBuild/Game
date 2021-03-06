using System;
using DigBuild.Engine.Math;
using DigBuild.Engine.Voxel;

namespace DigBuild.Voxel
{
    public class Chunk : IChunk
    {
        public ChunkPos Position { get; }
        public IBlockChunkStorage BlockStorage { get; }

        public Chunk(ChunkPos position, Action<Chunk> notifyUpdate)
        {
            Position = position;
            BlockStorage = new BlockChunkStorage(() => notifyUpdate(this));
        }
    }
}