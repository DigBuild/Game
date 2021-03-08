using DigBuild.Engine.Math;
using DigBuild.Engine.Voxel;

namespace DigBuild.Worldgen
{
    public sealed class ChunkPrototype : ChunkBase
    {
        public override ChunkPos Position { get; }

        internal ChunkPrototype(ChunkPos position)
        {
            Position = position;
        }
    }
}