using System;
using DigBuild.Audio;
using DigBuild.Blocks;
using DigBuild.Crafting;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Entities;
using DigBuild.Engine.Events;
using DigBuild.Engine.Items;
using DigBuild.Engine.Registries;
using DigBuild.Engine.Storage;
using DigBuild.Engine.Ticking;
using DigBuild.Engine.Worldgen;
using DigBuild.Engine.Worlds;
using DigBuild.Entities;
using DigBuild.Items;
using DigBuild.Render.Models.Geometry;
using DigBuild.Worldgen.Biomes;

namespace DigBuild.Registries
{
    public static class GameRegistries
    {
        public static Registry<IDataHandle<IWorld>> WorldStorageTypes { get; private set; } = null!;
        public static Registry<IDataHandle<IChunk>> ChunkStorageTypes { get; private set; } = null!;

        public static Registry<IJobHandle> Jobs { get; private set; } = null!;

        public static TypeRegistry<IBlockEvent, BlockEventInfo> BlockEvents { get; private set; } = null!;
        public static Registry<IBlockAttribute> BlockAttributes { get; private set; } = null!;
        public static Registry<IBlockCapability> BlockCapabilities { get; private set; } = null!;
        public static Registry<Block> Blocks { get; private set; } = null!;

        public static TypeRegistry<IItemEvent, ItemEventInfo> ItemEvents { get; private set; } = null!;
        public static Registry<IItemAttribute> ItemAttributes { get; private set; } = null!;
        public static Registry<IItemCapability> ItemCapabilities { get; private set; } = null!;
        public static Registry<Item> Items { get; private set; } = null!;

        public static TypeRegistry<IEntityEvent, EntityEventInfo> EntityEvents { get; private set; } = null!;
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
        public static Registry<IGeometryProvider> GeometryProviders { get; private set; } = null!;

        internal static void Initialize(EventBus eventBus)
        {
            var manager = new RegistryManager(eventBus);
            
            void MakeRegistry<T>(string name, Action<Registry<T>> assigner, Action<RegistryBuilder<T>>? builder = null)
                where T : notnull
            {
                manager.CreateRegistryOf<T>(DigBuildGame.Domain, name);
                eventBus.Subscribe<RegistryBuiltEvent<T>>(evt => assigner(evt.Registry));
                if (builder != null)
                    eventBus.Subscribe<RegistryBuildingEvent<T>>(evt => builder(evt.Registry));
            }
            void MakeTypeRegistry<T, TValue>(string name, Action<TypeRegistry<T, TValue>> assigner, Action<TypeRegistryBuilder<T, TValue>>? builder = null)
                where T : notnull
            {
                manager.CreateTypeRegistryOf<T, TValue>(DigBuildGame.Domain, name);
                eventBus.Subscribe<TypeRegistryBuiltEvent<T, TValue>>(evt => assigner(evt.Registry));
                if (builder != null)
                    eventBus.Subscribe<TypeRegistryBuildingEvent<T, TValue>>(evt => builder(evt.Registry));
            }
            
            MakeRegistry<IDataHandle<IWorld>>(
                "world_storage_type",
                reg => WorldStorageTypes = reg,
                GameWorldStorages.Register
            );
            MakeRegistry<IDataHandle<IChunk>>(
                "chunk_storage_type",
                reg => ChunkStorageTypes = reg,
                GameChunkStorages.Register
            );

            MakeRegistry<IJobHandle>(
                "jobs",
                reg => Jobs = reg,
                GameJobs.Register
            );

            MakeTypeRegistry<IBlockEvent, BlockEventInfo>(
                "block_events",
                reg => BlockEvents = reg,
                BlockEvent.Register
            );
            MakeRegistry<IBlockAttribute>(
                "block_attributes",
                reg => BlockAttributes = reg,
                Registries.BlockAttributes.Register
            );
            MakeRegistry<IBlockCapability>(
                "block_capabilities",
                reg => BlockCapabilities = reg
            );
            MakeRegistry<Block>(
                "blocks",
                reg => Blocks = reg
            );
            
            MakeTypeRegistry<IItemEvent, ItemEventInfo>(
                "item_events",
                reg => ItemEvents = reg,
                ItemEvent.Register
            );
            MakeRegistry<IItemAttribute>(
                "item_attributes",
                reg => ItemAttributes = reg,
                Registries.ItemAttributes.Register
            );
            MakeRegistry<IItemCapability>(
                "item_capabilities",
                reg => ItemCapabilities = reg,
                Registries.ItemCapabilities.Register
            );
            MakeRegistry<Item>(
                "items",
                reg => Items = reg
            );
            
            MakeTypeRegistry<IEntityEvent, EntityEventInfo>(
                "entity_events",
                reg => EntityEvents = reg,
                EntityEvent.Register
            );
            MakeRegistry<IEntityAttribute>(
                "entity_attributes",
                reg => EntityAttributes = reg,
                Registries.EntityAttributes.Register
            );
            MakeRegistry<IEntityCapability>(
                "entity_capabilities",
                reg => EntityCapabilities = reg,
                Registries.EntityCapabilities.Register
            );
            MakeRegistry<Entity>(
                "entities",
                reg => Entities = reg,
                GameEntities.Register
            );
            
            MakeRegistry<IWorldgenAttribute>(
                "worldgen_attributes",
                reg => WorldgenAttributes = reg
            );
            MakeRegistry<IWorldgenFeature>(
                "worldgen_features",
                reg => WorldgenFeatures = reg
            );

            MakeRegistry<IBiomeAttribute>(
                "biome_attributes",
                reg => BiomeAttributes = reg
            );
            MakeRegistry<IBiome>(
                "biomes",
                reg => Biomes = reg
            );

            MakeRegistry<ICraftingRecipe>(
                "crafting_recipes",
                reg => CraftingRecipes = reg
            );
            MakeRegistry<IParticleSystemData>(
                "particle_systems",
                reg => ParticleSystems = reg
            );
            MakeRegistry<Sound>(
                "sounds",
                reg => Sounds = reg,
                GameSounds.Register
            );
            MakeRegistry<IGeometryProvider>(
                "geometry_providers",
                reg => GeometryProviders = reg,
                GameGeometryProviders.Register
            );

            manager.BuildAll();
        }
    }
}