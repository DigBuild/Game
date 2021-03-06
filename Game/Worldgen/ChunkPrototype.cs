using DigBuild.Engine.Math;
using DigBuild.Engine.Voxel;
using DigBuild.Voxel;

namespace DigBuild.Worldgen
{
    public sealed class ChunkPrototype : IChunk
    {
        public ChunkPos Position { get; }
        public IBlockChunkStorage BlockStorage { get; } = new BlockChunkStorage(() => { });

        internal ChunkPrototype(ChunkPos position)
        {
            Position = position;
        }
    }
}