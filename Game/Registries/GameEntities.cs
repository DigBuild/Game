using DigBuild.Behaviors;
using DigBuild.Engine.Entities;
using DigBuild.Engine.Math;
using DigBuild.Engine.Registries;
using DigBuild.Platform.Resource;
using DigBuild.Players;

namespace DigBuild.Registries
{
    /// <summary>
    /// The game's entities.
    /// </summary>
    public static class GameEntities
    {
        /// <summary>
        /// The item entity.
        /// </summary>
        public static Entity Item { get; private set; } = null!;
        /// <summary>
        /// The player entity.
        /// </summary>
        public static Entity Player { get; private set; } = null!;

        internal static void Register(RegistryBuilder<Entity> registry)
        {
            Item = registry.Register(new ResourceName(DigBuildGame.Domain, "item"), builder =>
            {
                var itemData = builder.Add<ItemEntityData>();
                builder.Attach(new ItemEntityBehavior(), itemData);
                
                var physicalData = builder.Add<PhysicsEntityData>();
                builder.Attach(new PhysicsEntityBehavior(new AABB(-0.2f, 0, -0.2f, 0.2f, 0.4f, 0.2f)), physicalData);
            });
            Player = registry.Register(new ResourceName(DigBuildGame.Domain, "player"), builder =>
            {
                var physicalEntityData = builder.Add<PhysicsEntityData>();
                builder.Attach(new PhysicsEntityBehavior(
                    Players.Player.BoundingBox,
                    chunkLoadRadius: DigBuildGame.ViewRadius,
                    jumpForce: Players.Player.JumpForce,
                    jumpKickSpeed: Players.Player.JumpKickSpeed,
                    movementSpeedGround: Players.Player.MovementSpeedGround,
                    movementSpeedAir: Players.Player.MovementSpeedAir,
                    rotationSpeed: Players.Player.RotationSpeed
                ), physicalEntityData);

                var playerData = builder.Add<PlayerBehaviorData>();
                builder.Attach(new PlayerBehavior(), playerData);
                builder.Attach(new ItemPickupBehavior()
                {
                    PickupBounds = Players.Player.BoundingBox,
                    AttractionBounds = Players.Player.BoundingBox.Grow(3),
                }, playerData);
            });
        }
    }
}