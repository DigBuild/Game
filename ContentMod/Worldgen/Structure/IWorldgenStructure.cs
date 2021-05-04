using DigBuild.Engine.Math;
using DigBuild.Engine.Worlds;

namespace DigBuild.Content.Worldgen.Structure
{
    public interface IWorldgenStructure
    {
        Vector3I Min { get; }
        Vector3I Max { get; }

        void Place(Vector3I offset, IChunk chunk);
    }
}