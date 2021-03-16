using DigBuild.Engine.Entities;
using DigBuild.Engine.Reg;

namespace DigBuild.Entities
{
    public static class EntityEvent
    {
        public static void Register(ExtendedTypeRegistryBuilder<IEntityEvent, EntityEventInfo> registry)
        {
        }
    }

    public static class EntityEventExtensions
    {
    }
}