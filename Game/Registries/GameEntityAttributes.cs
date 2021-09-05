using System.Numerics;
using DigBuild.Engine.Entities;
using DigBuild.Engine.Items;
using DigBuild.Engine.Math;
using DigBuild.Engine.Registries;
using DigBuild.Platform.Resource;

namespace DigBuild.Registries
{
    /// <summary>
    /// The game's entity attributes.
    /// </summary>
    public class GameEntityAttributes
    {
        /// <summary>
        /// A position. Non-null. Defaults to <see cref="Vector3.Zero"/>.
        /// </summary>
        public static EntityAttribute<Vector3> Position { get; private set; } = null!;
        /// <summary>
        /// A bounding box. Nullable. Defaults to null.
        /// </summary>
        public static EntityAttribute<AABB?> Bounds { get; private set; } = null!;

        /// <summary>
        /// An item. Nullable. Defaults to null.
        /// </summary>
        public static EntityAttribute<IReadOnlyItemInstance?> Item { get; private set; } = null!;

        internal static void Register(RegistryBuilder<IEntityAttribute> registry)
        {
            Position = registry.Register(
                new ResourceName(DigBuildGame.Domain, "position"),
                Vector3.Zero
            );
            Bounds = registry.Register(
                new ResourceName(DigBuildGame.Domain, "bounds"),
                (AABB?) null
            );

            Item = registry.Register(
                new ResourceName(DigBuildGame.Domain, "item"),
                (IReadOnlyItemInstance?) null
            );
        }
    }
}