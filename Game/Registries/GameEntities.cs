using DigBuild.Behaviors;
using DigBuild.Engine.Entities;
using DigBuild.Engine.Math;
using DigBuild.Engine.Registries;
using DigBuild.Entities;
using DigBuild.Platform.Resource;

namespace DigBuild.Registries
{
    public static class GameEntities
    {
        public static Entity Item { get; private set; } = null!;
        public static Entity Player { get; private set; } = null!;

        internal static void Register(RegistryBuilder<Entity> registry)
        {
            Item = registry.Create(new ResourceName(Game.Domain, "item"), builder =>
            {
                var itemData = builder.Add<ItemEntityData>();
                builder.Attach(new ItemEntityBehavior(), itemData);
                
                var physicalData = builder.Add<PhysicalEntityData>();
                builder.Attach(new PhysicalEntityBehavior(new AABB(-0.2f, 0, -0.2f, 0.2f, 0.4f, 0.2f)), physicalData);
            });
            Player = registry.Create(new ResourceName(Game.Domain, "player"), builder =>
            {
                var data = builder.Add<PhysicalEntityData>();
                builder.Attach(new PhysicalEntityBehavior(
                    Players.Player.BoundingBox,
                    jumpForce: Players.Player.JumpForce,
                    jumpKickSpeed: Players.Player.JumpKickSpeed,
                    movementSpeedGround: Players.Player.MovementSpeedGround,
                    movementSpeedAir: Players.Player.MovementSpeedAir,
                    rotationSpeed: Players.Player.RotationSpeed
                ), data);
            });
        }
    }
}