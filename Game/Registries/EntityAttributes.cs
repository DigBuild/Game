using System.Numerics;
using DigBuild.Engine.Entities;
using DigBuild.Engine.Items;
using DigBuild.Engine.Registries;
using DigBuild.Platform.Resource;

namespace DigBuild.Registries
{
    public class EntityAttributes
    {
        public static EntityAttribute<Vector3?> Position { get; private set; } = null!;
        // public static EntityAttribute<ICollider?> Collider { get; private set; } = null!;

        public static EntityAttribute<ItemInstance?> Item { get; private set; } = null!;
        public static EntityAttribute<long?> ItemJoinWorldTime { get; private set; } = null!;
        
        internal static void Register(RegistryBuilder<IEntityAttribute> registry)
        {
            Position = registry.Register(
                new ResourceName(DigBuildGame.Domain, "position"),
                (Vector3?) null
            );
            // Collider = registry.Register(
            //     new ResourceName(Game.Domain, "collider"),
            //     (ICollider?) null
            // );
            Item = registry.Register(
                new ResourceName(DigBuildGame.Domain, "item"),
                (ItemInstance?) null
            );
            ItemJoinWorldTime = registry.Register(
                new ResourceName(DigBuildGame.Domain, "item_join_world_time"),
                (long?) null
            );
        }
    }
}