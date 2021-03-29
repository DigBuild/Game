using DigBuild.Engine.Math;
using DigBuild.Engine.Worlds;

namespace DigBuild.Worlds
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