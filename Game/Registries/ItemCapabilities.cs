using DigBuild.Engine.Items;
using DigBuild.Engine.Registries;
using DigBuild.Platform.Resource;
using DigBuild.Players;

namespace DigBuild.Registries
{
    public class ItemCapabilities
    {
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