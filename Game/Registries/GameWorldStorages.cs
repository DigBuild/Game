using DigBuild.Engine.Registries;
using DigBuild.Engine.Storage;
using DigBuild.Engine.Worlds;

namespace DigBuild.Registries
{
    /// <summary>
    /// The game's world storages.
    /// </summary>
    public static class GameWorldStorages
    {
        internal static void Register(RegistryBuilder<IDataHandle<IWorld>> registry)
        {
        }
    }
}