using DigBuild.Blocks;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Math;
using DigBuild.Engine.Voxel;
using DigBuild.Engine.Worldgen;

namespace DigBuild.Voxel
{
    public class World : WorldBase
    {
        public ChunkManager ChunkManager { get; }

        public World(WorldGenerator generator)
        {
            ChunkManager = new ChunkManager(generator);
        }

        public override IChunk? GetChunk(ChunkPos pos, bool load = true)
        {
            return ChunkManager.Get(pos, load);
        }

        public override void SetBlock(BlockPos pos, Block? block)
        {
            var current = GetBlock(pos);
            if (block == current) return;
            base.SetBlock(pos, block);
            NotifyNeighbors(pos);
        }

        public void NotifyNeighbors(BlockPos pos)
        {
            foreach (var face in BlockFaces.All)
            {
                var offset = pos.Offset(face);
                var block = GetBlock(offset);
                block?.OnNeighborChanged(new BlockContext(this, offset, block), new BlockEvent.NeighborChanged(face.GetOpposite()));
            }
        }
    }
}