using DigBuild.Engine.Registries;
using DigBuild.Engine.Worlds;
using DigBuild.Platform.Resource;
using DigBuild.Worlds;

namespace DigBuild.Registries
{
    public static class GameChunkStorages
    {
        public static void Register(RegistryBuilder<IChunkStorageType> registry)
        {
            IBlockLightStorage.Type = registry.Create<IReadOnlyBlockLightStorage, IBlockLightStorage>(
                new ResourceName(Game.Domain, "block_light"),
                () => new BlockLightStorage()
            );
        }
    }
}