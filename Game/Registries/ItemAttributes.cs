using DigBuild.Engine.Items;
using DigBuild.Engine.Registries;
using DigBuild.Items;
using DigBuild.Platform.Resource;

namespace DigBuild.Registries
{
    public class ItemAttributes
    {
        public static ItemAttribute<EquippableFlags> Equippable { get; private set; } = null!;

        internal static void Register(RegistryBuilder<IItemAttribute> registry)
        {
            Equippable = registry.Register(
                new ResourceName(DigBuildGame.Domain, "equippable_flags"),
                EquippableFlags.NotEquippable
            );
        }
    }
}