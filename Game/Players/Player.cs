using System;
using System.Numerics;
using DigBuild.Behaviors;
using DigBuild.Engine.Entities;
using DigBuild.Engine.Math;
using DigBuild.Engine.Worlds;
using DigBuild.Registries;

namespace DigBuild.Players
{
    public sealed class Player : IPlayer
    {
        internal static readonly AABB BoundingBox = new(-0.4f, 0, -0.4f, 0.4f, 1.85f, 0.4f);
        internal const float JumpForce = 12f * TickSource.TickDurationSeconds;
        internal const float JumpKickSpeed = 0.8f * TickSource.TickDurationSeconds;
        internal const float MovementSpeedGround = 6 * TickSource.TickDurationSeconds;
        internal const float MovementSpeedAir = 5 * TickSource.TickDurationSeconds;
        internal const float RotationSpeed = 4 * TickSource.TickDurationSeconds;
        
        public EntityInstance Entity { get; }

        public IPhysicalEntity PhysicalEntity => Entity.Type.Get(new EntityContext(Entity), EntityCapabilities.PhysicalEntity)!;
        public PlayerInventory Inventory { get; } = new();

        internal Player(EntityInstance entity)
        {
            Entity = entity;
        }
        
        public IPlayerCamera GetCamera(float partialTick)
        {
            var physicalEntity = PhysicalEntity;
            return new PlayerCamera(
                physicalEntity.Position + physicalEntity.Velocity * partialTick + Vector3.UnitY * 1.75f,
                physicalEntity.Pitch + physicalEntity.AngularVelocityPitch * partialTick,
                physicalEntity.Yaw + physicalEntity.AngularVelocityYaw * partialTick,
                MathF.PI / 2
            );
        }
    }

    public static class PlayerWorldExtensions
    {
        public static Player AddPlayer(this IWorld world, Vector3 position)
        {
            var entity = world.AddEntity(GameEntities.Player).WithPosition(position);
            return new Player(entity);
        }
    }
}