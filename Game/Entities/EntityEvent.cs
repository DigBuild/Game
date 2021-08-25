using DigBuild.Engine.Entities;
using DigBuild.Engine.Registries;

namespace DigBuild.Entities
{
    public static class EntityEvent
    {
        public static void Register(TypeRegistryBuilder<IEntityEvent, EntityEventInfo> registry)
        {
        }
    }

    public static class EntityEventExtensions
    {
    }
}