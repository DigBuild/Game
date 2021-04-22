using DigBuild.Engine.Events;
using DigBuild.Engine.Registries;

namespace DigBuild.Events
{
    public sealed class RegistryBuildingEvent<T> : IEvent where T : notnull
    {
        public RegistryBuilder<T> Registry { get; }

        public RegistryBuildingEvent(RegistryBuilder<T> registry)
        {
            Registry = registry;
        }
    }
}