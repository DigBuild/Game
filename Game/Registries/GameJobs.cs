using DigBuild.Behaviors;
using DigBuild.Engine.Registries;
using DigBuild.Engine.Ticking;
using DigBuild.Platform.Resource;

namespace DigBuild.Registries
{
    public static class GameJobs
    {
        public static Job<IPhysicalEntity> PhysicalEntityMove { get; private set; } = null!;

        internal static void Register(RegistryBuilder<IJob> registry)
        {
            PhysicalEntityMove = registry.RegisterParallel<IPhysicalEntity>(
                new ResourceName(DigBuildGame.Domain, "physical_entity_move"),
                PhysicalEntityBehavior.Update
            );
        }
    }
}