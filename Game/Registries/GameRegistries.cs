using DigBuild.Audio;
using DigBuild.Blocks;
using DigBuild.Engine;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.BuiltIn;
using DigBuild.Engine.Entities;
using DigBuild.Engine.Events;
using DigBuild.Engine.Items;
using DigBuild.Engine.Registries;
using DigBuild.Engine.Storage;
using DigBuild.Engine.Ticking;
using DigBuild.Engine.Worldgen;
using DigBuild.Engine.Worlds;
using DigBuild.Entities;
using DigBuild.Events;
using DigBuild.Items;
using DigBuild.Platform.Resource;
using DigBuild.Recipes;
using DigBuild.Worldgen.Biomes;

namespace DigBuild.Registries
{
    public static class GameRegistries
    {
        public static Registry<IDataHandle<IWorld>> WorldStorageTypes { get; private set; } = null!;
        public static Registry<IDataHandle<IChunk>> ChunkStorageTypes { get; private set; } = null!;

        public static Registry<IJobHandle> Jobs { get; private set; } = null!;

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
        public static Registry<IBiomeAttribute> BiomeAttributes { get; private set; } = null!;
        public static Registry<IBiome> Biomes { get; private set; } = null!;

        public static Registry<ICraftingRecipe> CraftingRecipes { get; private set; } = null!;
        public static Registry<IParticleSystemData> ParticleSystems { get; private set; } = null!;
        public static Registry<Sound> Sounds { get; private set; } = null!;

        internal static void Initialize(EventBus bus)
        {
            var manager = new RegistryManager();

            var worldStorageTypes = manager.CreateRegistryOf<IDataHandle<IWorld>>(
                new ResourceName(DigBuildGame.Domain, "world_storage_type")
            );
            worldStorageTypes.Building += DigBuildEngine.Register;
            worldStorageTypes.Building += GameWorldStorages.Register;
            worldStorageTypes.Building += reg => bus.Post(new RegistryBuildingEvent<IDataHandle<IWorld>>(reg));
            worldStorageTypes.Built += reg =>
            {
                BuiltInRegistries.WorldStorageTypes = reg;
                DataContainer<IWorld>.Registry = reg;
                WorldStorageTypes = reg;
            };

            var chunkStorageTypes = manager.CreateRegistryOf<IDataHandle<IChunk>>(
                new ResourceName(DigBuildGame.Domain, "chunk_storage_type")
            );
            chunkStorageTypes.Building += DigBuildEngine.Register;
            chunkStorageTypes.Building += GameChunkStorages.Register;
            chunkStorageTypes.Building += reg => bus.Post(new RegistryBuildingEvent<IDataHandle<IChunk>>(reg));
            chunkStorageTypes.Built += reg =>
            {
                BuiltInRegistries.ChunkStorageTypes = reg;
                DataContainer<IChunk>.Registry = reg;
                ChunkStorageTypes = reg;
            };
            
            var jobs = manager.CreateRegistryOf<IJobHandle>(
                new ResourceName(DigBuildGame.Domain, "jobs")
            );
            jobs.Building += GameJobs.Register;
            jobs.Building += reg => bus.Post(new RegistryBuildingEvent<IJobHandle>(reg));
            jobs.Built += reg => Jobs = reg;
            
            var blockEvents = manager.CreateExtendedRegistryOfTypes<IBlockEvent, BlockEventInfo>(
                new ResourceName(DigBuildGame.Domain, "block_events"), _ => true
            );
            blockEvents.Building += DigBuildEngine.Register;
            blockEvents.Building += BlockEvent.Register;
            // blockEvents.Building += reg => bus.Post(new ExtendedTypeRegistryBuildingEvent<IBlockEvent, BlockEventInfo>(reg));
            blockEvents.Built += reg =>
            {
                BuiltInRegistries.BlockEvents = reg;
                BlockEvents = reg;
            };
            
            var blockAttributes = manager.CreateRegistryOf<IBlockAttribute>(new ResourceName(DigBuildGame.Domain, "block_attributes"));
            blockAttributes.Building += DigBuildEngine.Register;
            blockAttributes.Building += Registries.BlockAttributes.Register;
            blockAttributes.Building += reg => bus.Post(new RegistryBuildingEvent<IBlockAttribute>(reg));
            blockAttributes.Built += reg =>
            {
                BuiltInRegistries.BlockAttributes = reg;
                BlockAttributes = reg;
            };
            
            var blockCapabilities = manager.CreateRegistryOf<IBlockCapability>(new ResourceName(DigBuildGame.Domain, "block_capabilities"));
            blockCapabilities.Building += Registries.BlockCapabilities.Register;
            blockCapabilities.Building += reg => bus.Post(new RegistryBuildingEvent<IBlockCapability>(reg));
            blockCapabilities.Built += reg =>
            {
                BuiltInRegistries.BlockCapabilities = reg;
                BlockCapabilities = reg;
            };
            
            var blocks = manager.CreateRegistryOf<Block>(new ResourceName(DigBuildGame.Domain, "blocks"));
            blocks.Building += reg => bus.Post(new RegistryBuildingEvent<Block>(reg));
            blocks.Built += reg =>
            {
                BuiltInRegistries.Blocks = reg;
                Blocks = reg;
            };
            
            
            var itemEvents = manager.CreateExtendedRegistryOfTypes<IItemEvent, ItemEventInfo>(
                new ResourceName(DigBuildGame.Domain, "item_events"), _ => true
            );
            itemEvents.Building += ItemEvent.Register;
            // itemEvents.Building += reg => bus.Post(new ExtendedTypeRegistryBuildingEvent<IItemEvent, ItemEventInfo>(reg));
            itemEvents.Built += reg =>
            {
                ItemEvents = reg;
                ItemRegistryBuilderExtensions.EventRegistry = reg;
            };
            
            var itemAttributes = manager.CreateRegistryOf<IItemAttribute>(new ResourceName(DigBuildGame.Domain, "item_attributes"));
            itemAttributes.Building += DigBuildEngine.Register;
            itemAttributes.Building += reg => bus.Post(new RegistryBuildingEvent<IItemAttribute>(reg));
            itemAttributes.Built += reg =>
            {
                ItemAttributes = reg;
                ItemRegistryBuilderExtensions.ItemAttributes = reg;
            };
            
            var itemCapabilities = manager.CreateRegistryOf<IItemCapability>(new ResourceName(DigBuildGame.Domain, "item_capabilities"));
            itemCapabilities.Building += reg => bus.Post(new RegistryBuildingEvent<IItemCapability>(reg));
            itemCapabilities.Built += reg =>
            {
                ItemCapabilities = reg;
                ItemRegistryBuilderExtensions.ItemCapabilities = reg;
            };
            
            var items = manager.CreateRegistryOf<Item>(new ResourceName(DigBuildGame.Domain, "items"));
            items.Building += reg => bus.Post(new RegistryBuildingEvent<Item>(reg));
            items.Built += reg => Items = reg;

            
            var entityEvents = manager.CreateExtendedRegistryOfTypes<IEntityEvent, EntityEventInfo>(
                new ResourceName(DigBuildGame.Domain, "entity_events"), _ => true
            );
            entityEvents.Building += DigBuildEngine.Register;
            entityEvents.Building += EntityEvent.Register;
            // entityEvents.Building += reg => bus.Post(new ExtendedTypeRegistryBuildingEvent<IEntityEvent, EntityEventInfo>(reg));
            entityEvents.Built += reg =>
            {
                EntityEvents = reg;
                EntityRegistryBuilderExtensions.EventRegistry = reg;
            };
            
            var entityAttributes = manager.CreateRegistryOf<IEntityAttribute>(new ResourceName(DigBuildGame.Domain, "entity_attributes"));
            entityAttributes.Building += DigBuildEngine.Register;
            entityAttributes.Building += Registries.EntityAttributes.Register;
            entityAttributes.Building += reg => bus.Post(new RegistryBuildingEvent<IEntityAttribute>(reg));
            entityAttributes.Built += reg =>
            {
                EntityAttributes = reg;
                EntityRegistryBuilderExtensions.EntityAttributes = reg;
            };
            
            var entityCapabilities = manager.CreateRegistryOf<IEntityCapability>(new ResourceName(DigBuildGame.Domain, "entity_capabilities"));
            entityCapabilities.Building += Registries.EntityCapabilities.Register;
            entityCapabilities.Building += reg => bus.Post(new RegistryBuildingEvent<IEntityCapability>(reg));
            entityCapabilities.Built += reg =>
            {
                EntityCapabilities = reg;
                EntityRegistryBuilderExtensions.EntityCapabilities = reg;
            };
            
            var entities = manager.CreateRegistryOf<Entity>(new ResourceName(DigBuildGame.Domain, "entities"));
            entities.Building += GameEntities.Register;
            entities.Building += reg => bus.Post(new RegistryBuildingEvent<Entity>(reg));
            entities.Built += reg => Entities = reg;


            var worldgenAttributes = manager.CreateRegistryOf<IWorldgenAttribute>(new ResourceName(DigBuildGame.Domain, "worldgen_attributes"));
            worldgenAttributes.Building += reg => bus.Post(new RegistryBuildingEvent<IWorldgenAttribute>(reg));
            worldgenAttributes.Built += reg => WorldgenAttributes = reg;

            var worldgenFeatures = manager.CreateRegistryOf<IWorldgenFeature>(new ResourceName(DigBuildGame.Domain, "worldgen_features"));
            worldgenFeatures.Building += reg => bus.Post(new RegistryBuildingEvent<IWorldgenFeature>(reg));
            worldgenFeatures.Built += reg => WorldgenFeatures = reg;

            var biomeAttributes = manager.CreateRegistryOf<IBiomeAttribute>(new ResourceName(DigBuildGame.Domain, "biome_attributes"));
            biomeAttributes.Building += reg => bus.Post(new RegistryBuildingEvent<IBiomeAttribute>(reg));
            biomeAttributes.Built += reg => BiomeAttributes = reg;

            var biomes = manager.CreateRegistryOf<IBiome>(new ResourceName(DigBuildGame.Domain, "biomes"));
            biomes.Building += reg => bus.Post(new RegistryBuildingEvent<IBiome>(reg));
            biomes.Built += reg => Biomes = reg;

            var craftingRecipes = manager.CreateRegistryOf<ICraftingRecipe>(new ResourceName(DigBuildGame.Domain, "crafting_recipes"));
            craftingRecipes.Building += reg => bus.Post(new RegistryBuildingEvent<ICraftingRecipe>(reg));
            craftingRecipes.Built += reg => CraftingRecipes = reg;

            var particleSystems = manager.CreateRegistryOf<IParticleSystemData>(new ResourceName(DigBuildGame.Domain, "particle_systems"));
            particleSystems.Building += reg => bus.Post(new RegistryBuildingEvent<IParticleSystemData>(reg));
            particleSystems.Built += reg => ParticleSystems = reg;

            var sounds = manager.CreateRegistryOf<Sound>(new ResourceName(DigBuildGame.Domain, "sounds"));
            sounds.Building += GameSounds.Register;
            sounds.Building += reg => bus.Post(new RegistryBuildingEvent<Sound>(reg));
            sounds.Built += reg => Sounds = reg;
            
            manager.BuildAll();
        }
    }
}