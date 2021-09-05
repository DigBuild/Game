using DigBuild.Behaviors;
using DigBuild.Engine.Registries;
using DigBuild.Engine.Ticking;
using DigBuild.Platform.Resource;

namespace DigBuild.Registries
{
    /// <summary>
    /// The game's jobs.
    /// </summary>
    public static class GameJobs
    {
        /// <summary>
        /// The physics entity movement job.
        /// </summary>
        public static Job<IPhysicsEntity> PhysicsEntityMove { get; private set; } = null!;

        internal static void Register(RegistryBuilder<IJob> registry)
        {
            PhysicsEntityMove = registry.RegisterParallel<IPhysicsEntity>(
                new ResourceName(DigBuildGame.Domain, "physics_entity_move"),
                PhysicsEntityBehavior.Update
            );
        }
    }
}