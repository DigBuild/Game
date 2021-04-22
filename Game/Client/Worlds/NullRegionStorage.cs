using System.Diagnostics.CodeAnalysis;
using DigBuild.Engine.Impl.Worlds;
using DigBuild.Engine.Math;
using DigBuild.Engine.Storage;
using DigBuild.Engine.Worlds;

namespace DigBuild.Client.Worlds
{
    public sealed class NullRegionStorage : IRegionStorage
    {
        public static IRegionStorage Instance { get; } = new NullRegionStorage();

        public bool TryLoad(RegionChunkPos pos, [NotNullWhen(true)] out Chunk? chunk)
        {
            chunk = null;
            return false;
        }

        public void Save(Chunk chunk)
        {
        }

        public DataContainer<IRegion> LoadOrCreateManagedData()
        {
            return new();
        }

        public ILowDensityRegion LoadOrCreateManagedLowDensity()
        {
            throw new System.NotImplementedException();
        }
    }
}