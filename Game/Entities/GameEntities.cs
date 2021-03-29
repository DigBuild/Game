using DigBuild.Behaviors;
using DigBuild.Engine.Entities;
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
    }
}