using System;
using System.Numerics;
using DigBuild.Behaviors;
using DigBuild.Engine.Entities;
using DigBuild.Engine.Items;
using DigBuild.Engine.Registries;
using DigBuild.Platform.Resource;
using DigBuild.Players;

namespace DigBuild.Registries
{
    /// <summary>
    /// The game's entity capabilities.
    /// </summary>
    public class GameEntityCapabilities
    {
        /// <summary>
        /// A physics entity. Nullable. Defaults to null.
        /// </summary>
        public static EntityCapability<IPhysicsEntity?> PhysicsEntity { get; private set; } = null!;

        /// <summary>
        /// An item entity. Nullable. Defaults to null.
        /// </summary>
        public static EntityCapability<IItemEntity?> ItemEntity { get; private set; } = null!;
        
        /// <summary>
        /// A player entity. Nullable. Defaults to null.
        /// </summary>
        public static EntityCapability<IPlayerEntity?> PlayerEntity { get; private set; } = null!;
        
        internal static void Register(RegistryBuilder<IEntityCapability> registry)
        {
            PhysicsEntity = registry.Register(
                new ResourceName(DigBuildGame.Domain, "physical_entity"),
                (IPhysicsEntity?) null
            );

            ItemEntity = registry.Register(
                new ResourceName(DigBuildGame.Domain, "item_entity"),
                (IItemEntity?) null
            );

            PlayerEntity = registry.Register(
                new ResourceName(DigBuildGame.Domain, "player_entity"),
                (IPlayerEntity?) null
            );
        }
    }

    /// <summary>
    /// Helpers for entity capabilities.
    /// </summary>
    public static class GameEntityCapabilityExtensions
    {
        /// <summary>
        /// Sets the position of a physics entity.
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="position">The position</param>
        /// <returns>The entity</returns>
        public static EntityInstance WithPosition(this EntityInstance entity, Vector3 position)
        {
            var physicalEntity = entity.Get(GameEntityCapabilities.PhysicsEntity);
            if (physicalEntity == null)
                throw new ArgumentException("Cannot set position on a non-physical entity.");
            physicalEntity.Position = position;
            // Prevent teleportation jitter
            physicalEntity.PrevPosition = position;
            physicalEntity.PrevVelocity = Vector3.Zero;
            return entity;
        }

        /// <summary>
        /// Sets the item of an item entity.
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="item">The item</param>
        /// <returns>The entity</returns>
        public static EntityInstance WithItem(this EntityInstance entity, ItemInstance item)
        {
            var itemEntity = entity.Get(GameEntityCapabilities.ItemEntity);
            if (itemEntity == null)
                throw new ArgumentException("Cannot set item on a non-item entity.");
            itemEntity.Item = item;
            return entity;
        }
    }
}