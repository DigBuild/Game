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
    /// <summary>
    /// A player.
    /// </summary>
    public sealed class Player : IPlayer
    {
        /// <summary>
        /// The bounding box.
        /// </summary>
        public static readonly AABB BoundingBox = new(-0.4f, 0, -0.4f, 0.4f, 1.85f, 0.4f);
        /// <summary>
        /// The camera height.
        /// </summary>
        public const float CameraHeight = 1.75f;

        /// <summary>
        /// The jump force.
        /// </summary>
        public const float JumpForce = 12f * TickSource.TickDurationSeconds; 
        /// <summary>
        /// The jump kick speed.
        /// </summary>
        public const float JumpKickSpeed = 0.8f * TickSource.TickDurationSeconds; 
        /// <summary>
        /// The ground speed.
        /// </summary>
        public const float MovementSpeedGround = 6 * TickSource.TickDurationSeconds; 
        /// <summary>
        /// The air speed.
        /// </summary>
        public const float MovementSpeedAir = 5 * TickSource.TickDurationSeconds; 
        /// <summary>
        /// The rotation speed.
        /// </summary>
        public const float RotationSpeed = 4 * TickSource.TickDurationSeconds;

        public GameplayController GameplayController { get; }

        public EntityInstance Entity { get; }
        public IPhysicsEntity PhysicsEntity => Entity.Get(GameEntityCapabilities.PhysicsEntity)!;
        public IPlayerInventory Inventory => Entity.Get(GameEntityCapabilities.PlayerEntity)!.Inventory;
        public IPlayerState State => Entity.Get(GameEntityCapabilities.PlayerEntity)!.State;

        public Player(GameplayController gameplayController, EntityInstance entity)
        {
            GameplayController = gameplayController;
            Entity = entity;
        }

        public IPlayerCamera GetCamera(float partialTick)
        {
            var physicalEntity = PhysicsEntity;
            var eyePosition = physicalEntity.PrevPosition + physicalEntity.PrevVelocity * partialTick + Vector3.UnitY * CameraHeight;
            var blockPos = new BlockPos(eyePosition);
            var eyeBlock = Entity.World.GetBlock(blockPos);

            var underwater = eyeBlock != null && eyeBlock.Get(Entity.World, blockPos, GameBlockAttributes.Water);

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