using DigBuild.Behaviors;
using DigBuild.Engine.Entities;
using DigBuild.Engine.Math;
using DigBuild.Engine.Registries;
using DigBuild.Platform.Resource;
using DigBuild.Players;

namespace DigBuild.Registries
{
    public static class GameEntities
    {
        public static Entity Item { get; private set; } = null!;
        public static Entity Player { get; private set; } = null!;

        internal static void Register(RegistryBuilder<Entity> registry)
        {
            Item = registry.Create(new ResourceName(DigBuildGame.Domain, "item"), builder =>
            {
                var itemData = builder.Add<ItemEntityData>();
                builder.Attach(new ItemEntityBehavior(), itemData);
                
                var physicalData = builder.Add<PhysicalEntityData>();
                builder.Attach(new PhysicalEntityBehavior(new AABB(-0.2f, 0, -0.2f, 0.2f, 0.4f, 0.2f)), physicalData);
            });
            Player = registry.Create(new ResourceName(DigBuildGame.Domain, "player"), builder =>
            {
                var physicalEntityData = builder.Add<PhysicalEntityData>();
                builder.Attach(new PhysicalEntityBehavior(
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
            });
        }
    }
}