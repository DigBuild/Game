using System;
using System.Numerics;
using DigBuild.Behaviors;
using DigBuild.Controller;
using DigBuild.Engine.Entities;
using DigBuild.Engine.Math;
using DigBuild.Engine.Worlds.Impl;
using DigBuild.Registries;

namespace DigBuild.Players
{
    public interface IPlayer
    {
        GameplayController GameplayController { get; }

        EntityInstance Entity { get; }
        IPhysicalEntity PhysicalEntity { get; }
        IPlayerInventory Inventory { get; }
        IPlayerState State { get; }

        IPlayerCamera GetCamera(float partialTick);
    }

    public sealed class Player : IPlayer
    {
        public static readonly AABB BoundingBox = new(-0.4f, 0, -0.4f, 0.4f, 1.85f, 0.4f);
        public const float CameraHeight = 1.75f;

        public const float JumpForce = 12f * TickSource.TickDurationSeconds; 
        public const float JumpKickSpeed = 0.8f * TickSource.TickDurationSeconds; 
        public const float MovementSpeedGround = 6 * TickSource.TickDurationSeconds; 
        public const float MovementSpeedAir = 5 * TickSource.TickDurationSeconds; 
        public const float RotationSpeed = 4 * TickSource.TickDurationSeconds;

        public GameplayController GameplayController { get; }

        public EntityInstance Entity { get; }
        public IPhysicalEntity PhysicalEntity => Entity.Get(EntityCapabilities.PhysicalEntity)!;
        public IPlayerInventory Inventory => Entity.Get(EntityCapabilities.PlayerEntity)!.Inventory;
        public IPlayerState State => Entity.Get(EntityCapabilities.PlayerEntity)!.State;

        public Player(GameplayController gameplayController, EntityInstance entity)
        {
            GameplayController = gameplayController;
            Entity = entity;
        }

        public IPlayerCamera GetCamera(float partialTick)
        {
            var physicalEntity = PhysicalEntity;
            var eyePosition = physicalEntity.PrevPosition + physicalEntity.PrevVelocity * partialTick + Vector3.UnitY * CameraHeight;
            var blockPos = new BlockPos(eyePosition);
            var eyeBlock = Entity.World.GetBlock(blockPos);

            var underwater = eyeBlock != null && eyeBlock.Get(Entity.World, blockPos, BlockAttributes.Water);

            return new PlayerCamera(
                eyePosition,
                physicalEntity.PrevPitch + physicalEntity.AngularVelocityPitch * partialTick,
                physicalEntity.PrevYaw + physicalEntity.AngularVelocityYaw * partialTick,
                MathF.PI / 2,
                underwater
            );
        }
    }
}