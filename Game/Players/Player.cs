using System;
using System.Numerics;
using DigBuild.Behaviors;
using DigBuild.Engine.Entities;
using DigBuild.Engine.Math;
using DigBuild.Registries;

namespace DigBuild.Players
{
    public interface IPlayer
    {
        EntityInstance Entity { get; }
        IPhysicalEntity PhysicalEntity { get; }
        IPlayerInventory Inventory { get; }
        IPlayerState State { get; }

        IPlayerCamera GetCamera(float partialTick);
    }

    public sealed class Player : IPlayer
    {
        public static readonly AABB BoundingBox = new(-0.4f, 0, -0.4f, 0.4f, 1.85f, 0.4f); 
        public const float JumpForce = 12f * TickSource.TickDurationSeconds; 
        public const float JumpKickSpeed = 0.8f * TickSource.TickDurationSeconds; 
        public const float MovementSpeedGround = 6 * TickSource.TickDurationSeconds; 
        public const float MovementSpeedAir = 5 * TickSource.TickDurationSeconds; 
        public const float RotationSpeed = 4 * TickSource.TickDurationSeconds; 

        public EntityInstance Entity { get; }
        public IPhysicalEntity PhysicalEntity => Entity.Get(EntityCapabilities.PhysicalEntity)!;
        public IPlayerInventory Inventory => Entity.Get(EntityCapabilities.PlayerEntity)!.Inventory;
        public IPlayerState State => Entity.Get(EntityCapabilities.PlayerEntity)!.State;

        public Player(EntityInstance entity)
        {
            Entity = entity;
        }

        public IPlayerCamera GetCamera(float partialTick)
        {
            var physicalEntity = PhysicalEntity;
            return new PlayerCamera(
                physicalEntity.PrevPosition + physicalEntity.PrevVelocity * partialTick + Vector3.UnitY * 1.75f,
                physicalEntity.PrevPitch + physicalEntity.AngularVelocityPitch * partialTick,
                physicalEntity.PrevYaw + physicalEntity.AngularVelocityYaw * partialTick,
                MathF.PI / 2
            );
        }
    }
}