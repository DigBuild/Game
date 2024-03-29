﻿using System;
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
using DigBuild.Particles;
using DigBuild.Render.Models.Geometry;
using DigBuild.Worldgen.Biomes;

namespace DigBuild.Registries
{
    /// <summary>
    /// The game's registries.
    /// </summary>
    public static class GameRegistries
    {
        /// <summary>
        /// The world storage type registry.
        /// </summary>
        public static Registry<IDataHandle<IWorld>> WorldStorageTypes { get; private set; } = null!;
        /// <summary>
        /// The chunk storage type registry.
        /// </summary>
        public static Registry<IDataHandle<IChunk>> ChunkStorageTypes { get; private set; } = null!;

        /// <summary>
        /// The job registry.
        /// </summary>
        public static Registry<IJob> Jobs { get; private set; } = null!;

        /// <summary>
        /// The block event registry.
        /// </summary>
        public static TypeRegistry<IBlockEvent, BlockEventInfo> BlockEvents { get; private set; } = null!;
        /// <summary>
        /// The block attribute registry.
        /// </summary>
        public static Registry<IBlockAttribute> BlockAttributes { get; private set; } = null!;
        /// <summary>
        /// The block capability registry.
        /// </summary>
        public static Registry<IBlockCapability> BlockCapabilities { get; private set; } = null!;
        /// <summary>
        /// The block registry.
        /// </summary>
        public static Registry<Block> Blocks { get; private set; } = null!;

        /// <summary>
        /// The item event registry.
        /// </summary>
        public static TypeRegistry<IItemEvent, ItemEventInfo> ItemEvents { get; private set; } = null!;
        /// <summary>
        /// The item attribute registry
        /// </summary>
        public static Registry<IItemAttribute> ItemAttributes { get; private set; } = null!;
        /// <summary>
        /// The item capability registry.
        /// </summary>
        public static Registry<IItemCapability> ItemCapabilities { get; private set; } = null!;
        /// <summary>
        /// The item registry.
        /// </summary>
        public static Registry<Item> Items { get; private set; } = null!;

        /// <summary>
        /// The entity event registry.
        /// </summary>
        public static TypeRegistry<IEntityEvent, EntityEventInfo> EntityEvents { get; private set; } = null!;
        /// <summary>
        /// The entity attribute registry.
        /// </summary>
        public static Registry<IEntityAttribute> EntityAttributes { get; private set; } = null!;
        /// <summary>
        /// The entity capability registry.
        /// </summary>
        public static Registry<IEntityCapability> EntityCapabilities { get; private set; } = null!;
        /// <summary>
        /// The entity registry.
        /// </summary>
        public static Registry<Entity> Entities { get; private set; } = null!;

        /// <summary>
        /// The worldgen attribute registry.
        /// </summary>
        public static Registry<IWorldgenAttribute> WorldgenAttributes { get; private set; } = null!;
        /// <summary>
        /// The worldgen feature registry.
        /// </summary>
        public static Registry<IWorldgenFeature> WorldgenFeatures { get; private set; } = null!;
        /// <summary>
        /// The biome attribute registry.
        /// </summary>
        public static Registry<IBiomeAttribute> BiomeAttributes { get; private set; } = null!;
        /// <summary>
        /// The biome registry.
        /// </summary>
        public static Registry<IBiome> Biomes { get; private set; } = null!;

        /// <summary>
        /// The crafting recipe registry.
        /// </summary>
        public static Registry<ICraftingRecipe> CraftingRecipes { get; private set; } = null!;

        /// <summary>
        /// The particle system registry.
        /// </summary>
        public static Registry<IParticleSystemData> ParticleSystems { get; private set; } = null!;

        /// <summary>
        /// The sound registry.
        /// </summary>
        public static Registry<Sound> Sounds { get; private set; } = null!;
        
        /// <summary>
        /// The geometry provider registry.
        /// </summary>
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

            MakeRegistry<IJob>(
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
                Registries.GameBlockAttributes.Register
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
                Registries.GameItemAttributes.Register
            );
            MakeRegistry<IItemCapability>(
                "item_capabilities",
                reg => ItemCapabilities = reg,
                Registries.GameItemCapabilities.Register
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
                Registries.GameEntityAttributes.Register
            );
            MakeRegistry<IEntityCapability>(
                "entity_capabilities",
                reg => EntityCapabilities = reg,
                Registries.GameEntityCapabilities.Register
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