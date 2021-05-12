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
    public class EntityCapabilities
    {
        public static EntityCapability<IPhysicalEntity?> PhysicalEntity { get; private set; } = null!;
        public static EntityCapability<IItemEntity?> ItemEntity { get; private set; } = null!;

        public static EntityCapability<IPlayerEntity?> PlayerEntity { get; private set; } = null!;
        
        internal static void Register(RegistryBuilder<IEntityCapability> registry)
        {
            PhysicalEntity = registry.Register(
                new ResourceName(DigBuildGame.Domain, "physical_entity"),
                (IPhysicalEntity?) null
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

    public static class EntityCapabilityExtensions
    {
        public static EntityInstance WithPosition(this EntityInstance entity, Vector3 position)
        {
            var physicalEntity = entity.Get(EntityCapabilities.PhysicalEntity);
            if (physicalEntity == null)
                throw new ArgumentException("Cannot set position on a non-physical entity.");
            physicalEntity.Position = position;
            return entity;
        }
        public static EntityInstance WithItem(this EntityInstance entity, ItemInstance item)
        {
            var itemEntity = entity.Get(EntityCapabilities.ItemEntity);
            if (itemEntity == null)
                throw new ArgumentException("Cannot set item on a non-item entity.");
            itemEntity.Item = item;
            return entity;
        }
    }
}