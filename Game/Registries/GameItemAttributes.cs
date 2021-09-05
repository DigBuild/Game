using DigBuild.Engine.Items;
using DigBuild.Engine.Registries;
using DigBuild.Items;
using DigBuild.Platform.Resource;

namespace DigBuild.Registries
{
    /// <summary>
    /// The game's item attributes.
    /// </summary>
    public class GameItemAttributes
    {
        /// <summary>
        /// An equippability. Non-null. Defaults to <see cref="EquippableFlags.NotEquippable"/>.
        /// </summary>
        public static ItemAttribute<EquippableFlags> Equippable { get; private set; } = null!;

        /// <summary>
        /// A maximum stack size. Non-null. Defaults to 99.
        /// </summary>
        public static ItemAttribute<ushort> MaxStackSize { get; private set; } = null!;

        internal static void Register(RegistryBuilder<IItemAttribute> registry)
        {
            Equippable = registry.Register(
                new ResourceName(DigBuildGame.Domain, "equippable_flags"),
                EquippableFlags.NotEquippable
            );

            MaxStackSize = registry.Register(
                new ResourceName(DigBuildGame.Domain, "max_stack_size"),
                (ushort) 99u
            );
        }
    }
}