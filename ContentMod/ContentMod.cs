using DigBuild.Content.Models.Blocks;
using DigBuild.Content.Registries;
using DigBuild.Crafting;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Entities;
using DigBuild.Engine.Events;
using DigBuild.Engine.Items;
using DigBuild.Engine.Registries;
using DigBuild.Engine.Worldgen;
using DigBuild.Events;
using DigBuild.Modding;
using DigBuild.Registries;
using DigBuild.Worldgen.Biomes;
using GameEntities = DigBuild.Content.Registries.GameEntities;

namespace DigBuild.Content
{
    public sealed class ContentMod : IMod
    {
        public string Domain => DigBuildGame.Domain;

        public void Setup(EventBus bus)
        {
            bus.Subscribe<RegistryBuildingEvent<IBlockCapability>>(evt => BlockCapabilities.Register(evt.Registry));
            bus.Subscribe<RegistryBuildingEvent<Block>>(evt => GameBlocks.Register(evt.Registry));
            bus.Subscribe<RegistryBuildingEvent<Entity>>(evt => GameEntities.Register(evt.Registry));
            bus.Subscribe<RegistryBuildingEvent<Item>>(evt => GameItems.Register(evt.Registry));
            bus.Subscribe<RegistryBuildingEvent<ICraftingRecipe>>(evt => GameRecipes.Register(evt.Registry));
            bus.Subscribe<RegistryBuildingEvent<IWorldgenAttribute>>(evt => WorldgenAttributes.Register(evt.Registry));
            bus.Subscribe<RegistryBuildingEvent<IWorldgenFeature>>(evt => WorldgenFeatures.Register(evt.Registry));
            bus.Subscribe<RegistryBuildingEvent<IBiome>>(evt => GameBiomes.Register(evt.Registry));
            bus.Subscribe<RegistryBuildingEvent<IBiomeAttribute>>(evt => BiomeAttributes.Register(evt.Registry));
            bus.Subscribe<RegistryBuildingEvent<IParticleSystemData>>(ParticleSystems.Register);

            bus.Subscribe<ModelsBakedEvent>(OnModelsBaked);
        }

        private void OnModelsBaked(ModelsBakedEvent evt)
        {
            evt.ModelManager[GameBlocks.Water] = new WaterModel(evt.ModelManager[GameBlocks.Water]);
        }
    }
}