using DigBuild.Engine.Impl.Worlds;
using DigBuild.Engine.Registries;
using DigBuild.Engine.Storage;
using DigBuild.Engine.Worlds;
using DigBuild.Platform.Resource;
using DigBuild.Worlds;

namespace DigBuild.Registries
{
    public static class GameChunkStorages
    {
        public static void Register(RegistryBuilder<IDataHandle<IChunk>> registry)
        {
            IChunkBlockLight.Type = registry.Create<IChunk, IReadOnlyChunkBlockLight, IChunkBlockLight>(
                new ResourceName(Game.Domain, "block_light"),
                () => new ChunkChunkBlockLight()
            );
        }
    }
}