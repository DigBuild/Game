using DigBuild.Engine.Entities;
using DigBuild.Engine.Registries;

namespace DigBuild.Entities
{
    /// <summary>
    /// The game's entity events.
    /// </summary>
    public static class EntityEvent
    {
        internal static void Register(TypeRegistryBuilder<IEntityEvent, EntityEventInfo> registry)
        {
        }
    }
    
    /// <summary>
    /// Registration/subscription extensions for entity events.
    /// </summary>
    public static class EntityEventExtensions
    {
    }
}