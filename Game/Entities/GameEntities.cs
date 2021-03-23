using System.Numerics;
using DigBuild.Engine.Entities;
using DigBuild.Engine.Items;
using DigBuild.Engine.Registries;
using DigBuild.Platform.Resource;

namespace DigBuild.Entities
{
    public static class GameEntities
    {
        public static Entity Item { get; private set; } = null!;

        internal static void Register(RegistryBuilder<Entity> registry)
        {
            Item = registry.Create(new ResourceName(Game.Domain, "item"), builder =>
            {
                var data = builder.Add<ItemEntityData>();
                builder.Attach(new ItemEntityBehavior(), data);
                builder.Attach(new PhysicalEntityBehavior(), data);
            });
        }

        private class ItemEntityData : IItemEntityBehavior, IPhysicalEntityBehavior
        {
            public ItemInstance Item { get; set; } = ItemInstance.Empty;
            public long JoinWorldTime { get; set; }
            IItemEntity? IItemEntityBehavior.Capability { get; set; }

            public Vector3 Position { get; set; }
            IPhysicalEntity? IPhysicalEntityBehavior.Capability { get; set; }
        }
    }
}