using DigBuild.Behaviors;
using DigBuild.Engine.Registries;
using DigBuild.Engine.Ticking;
using DigBuild.Platform.Resource;

namespace DigBuild.Registries
{
    public static class GameJobs
    {
        public static JobHandle<IPhysicalEntity> PhysicalEntityMove { get; private set; } = null!;

        internal static void Register(RegistryBuilder<IJobHandle> registry)
        {
            PhysicalEntityMove = registry.CreateParallel<IPhysicalEntity>(
                new ResourceName(DigBuildGame.Domain, "physical_entity_move"),
                PhysicalEntityBehavior.Update
            );
        }
    }
}