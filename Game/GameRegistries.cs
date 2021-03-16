﻿using DigBuild.Blocks;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Entities;
using DigBuild.Engine.Items;
using DigBuild.Engine.Reg;
using DigBuild.Engine.Worldgen;
using DigBuild.Entities;
using DigBuild.Items;
using DigBuild.Platform.Resource;
using ItemEvent = DigBuild.Items.ItemEvent;

namespace DigBuild
{
    public static class GameRegistries
    {
        public static ExtendedTypeRegistry<IBlockEvent, BlockEventInfo> BlockEvents { get; private set; } = null!;
        public static Registry<IBlockAttribute> BlockAttributes { get; private set; } = null!;
        public static Registry<IBlockCapability> BlockCapabilities { get; private set; } = null!;
        public static Registry<Block> Blocks { get; private set; } = null!;

        public static ExtendedTypeRegistry<IItemEvent, ItemEventInfo> ItemEvents { get; private set; } = null!;
        public static Registry<IItemAttribute> ItemAttributes { get; private set; } = null!;
        public static Registry<IItemCapability> ItemCapabilities { get; private set; } = null!;
        public static Registry<Item> Items { get; private set; } = null!;

        public static ExtendedTypeRegistry<IEntityEvent, EntityEventInfo> EntityEvents { get; private set; } = null!;
        public static Registry<IEntityAttribute> EntityAttributes { get; private set; } = null!;
        public static Registry<IEntityCapability> EntityCapabilities { get; private set; } = null!;
        public static Registry<Entity> Entities { get; private set; } = null!;


        public static Registry<IWorldgenAttribute> WorldgenAttributes { get; private set; } = null!;
        public static Registry<IWorldgenFeature> WorldgenFeatures { get; private set; } = null!;

        internal static void Initialize()
        {
            var manager = new RegistryManager();
            
            var blockEvents = manager.CreateExtendedRegistryOfTypes<IBlockEvent, BlockEventInfo>(
                new ResourceName(Game.Domain, "block_events"), t => true
            );
            blockEvents.Building += BlockEvent.Register;
            blockEvents.Built += reg =>
            {
                BlockEvents = reg;
                BlockRegistryBuilderExtensions.EventRegistry = reg;
            };
            
            var blockAttributes = manager.CreateRegistryOf<IBlockAttribute>(new ResourceName(Game.Domain, "block_attributes"));
            blockAttributes.Building += DigBuild.Blocks.BlockAttributes.Register;
            blockAttributes.Built += reg =>
            {
                BlockAttributes = reg;
                BlockRegistryBuilderExtensions.BlockAttributes = reg;
            };
            
            var blockCapabilities = manager.CreateRegistryOf<IBlockCapability>(new ResourceName(Game.Domain, "block_capabilities"));
            // blockCapabilities.Building += DigBuild.Blocks.BlockAttributes.Register;
            blockCapabilities.Built += reg =>
            {
                BlockCapabilities = reg;
                BlockRegistryBuilderExtensions.BlockCapabilities = reg;
            };
            
            var blocks = manager.CreateRegistryOf<Block>(new ResourceName(Game.Domain, "blocks"));
            blocks.Building += GameBlocks.Register;
            blocks.Built += reg => Blocks = reg;
            
            
            var itemEvents = manager.CreateExtendedRegistryOfTypes<IItemEvent, ItemEventInfo>(
                new ResourceName(Game.Domain, "item_events"), t => true
            );
            itemEvents.Building += ItemEvent.Register;
            itemEvents.Built += reg =>
            {
                ItemEvents = reg;
                ItemRegistryBuilderExtensions.EventRegistry = reg;
            };
            
            var itemAttributes = manager.CreateRegistryOf<IItemAttribute>(new ResourceName(Game.Domain, "item_attributes"));
            // itemAttributes.Building += DigBuild.Items.ItemAttributes.Register;
            itemAttributes.Built += reg =>
            {
                ItemAttributes = reg;
                ItemRegistryBuilderExtensions.ItemAttributes = reg;
            };
            
            var itemCapabilities = manager.CreateRegistryOf<IItemCapability>(new ResourceName(Game.Domain, "item_capabilities"));
            // itemCapabilities.Building += DigBuild.Items.ItemAttributes.Register;
            itemCapabilities.Built += reg =>
            {
                ItemCapabilities = reg;
                ItemRegistryBuilderExtensions.ItemCapabilities = reg;
            };
            
            var items = manager.CreateRegistryOf<Item>(new ResourceName(Game.Domain, "items"));
            items.Building += GameItems.Register;
            items.Built += reg => Items = reg;

            
            var entityEvents = manager.CreateExtendedRegistryOfTypes<IEntityEvent, EntityEventInfo>(
                new ResourceName(Game.Domain, "entity_events"), t => true
            );
            entityEvents.Building += EntityEvent.Register;
            entityEvents.Built += reg =>
            {
                EntityEvents = reg;
                EntityRegistryBuilderExtensions.EventRegistry = reg;
            };
            
            var entityAttributes = manager.CreateRegistryOf<IEntityAttribute>(new ResourceName(Game.Domain, "entity_attributes"));
            entityAttributes.Building += DigBuild.Entities.EntityAttributes.Register;
            entityAttributes.Built += reg =>
            {
                EntityAttributes = reg;
                EntityRegistryBuilderExtensions.EntityAttributes = reg;
            };
            
            var entityCapabilities = manager.CreateRegistryOf<IEntityCapability>(new ResourceName(Game.Domain, "entity_capabilities"));
            entityCapabilities.Building += DigBuild.Entities.EntityCapabilities.Register;
            entityCapabilities.Built += reg =>
            {
                EntityCapabilities = reg;
                EntityRegistryBuilderExtensions.EntityCapabilities = reg;
            };
            
            var entities = manager.CreateRegistryOf<Entity>(new ResourceName(Game.Domain, "entities"));
            entities.Building += GameEntities.Register;
            entities.Built += reg => Entities = reg;


            var worldgenAttributes = manager.CreateRegistryOf<IWorldgenAttribute>(new ResourceName(Game.Domain, "worldgen_attributes"));
            worldgenAttributes.Building += Worldgen.WorldgenAttributes.Register;
            worldgenAttributes.Built += reg => WorldgenAttributes = reg;

            var worldgenFeatures = manager.CreateRegistryOf<IWorldgenFeature>(new ResourceName(Game.Domain, "worldgen_features"));
            worldgenFeatures.Building += Worldgen.WorldgenFeatures.Register;
            worldgenFeatures.Built += reg => WorldgenFeatures = reg;

            manager.BuildAll();
        }
    }
}