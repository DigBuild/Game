using DigBuild.Engine.Math;
using DigBuild.Engine.Voxel;

namespace DigBuild.Voxel
{
    public sealed class Chunk : ChunkBase
    {
        public override ChunkPos Position { get; }

        public Chunk(ChunkPos position)
        {
            Position = position;
        }
    }
}