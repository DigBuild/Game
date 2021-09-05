using DigBuild.Engine.Items;
using DigBuild.Engine.Registries;
using DigBuild.Platform.Resource;
using DigBuild.Players;

namespace DigBuild.Registries
{
    /// <summary>
    /// The game's item capabilities.
    /// </summary>
    public class GameItemCapabilities
    {
        /// <summary>
        /// An inventory extension. Nullable. Defaults to null.
        /// </summary>
        public static ItemCapability<IPlayerInventoryExtension?> InventoryExtension { get; private set; } = null!;

        internal static void Register(RegistryBuilder<IItemCapability> registry)
        {
            InventoryExtension = registry.Register(
                new ResourceName(DigBuildGame.Domain, "inventory_extension"),
                (IPlayerInventoryExtension?) null
            );
        }
    }
}