using DigBuild.Content.Registries;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Entities;
using DigBuild.Engine.Events;
using DigBuild.Engine.Items;
using DigBuild.Engine.Worldgen;
using DigBuild.Events;
using DigBuild.Modding;
using DigBuild.Recipes;
using DigBuild.Registries;
using GameEntities = DigBuild.Content.Registries.GameEntities;

namespace DigBuild.Content
{
    public sealed class ContentMod : IMod
    {
        public void Setup(EventBus bus)
        {
            bus.Subscribe<RegistryBuildingEvent<Block>>(evt => GameBlocks.Register(evt.Registry));
            bus.Subscribe<RegistryBuildingEvent<Entity>>(evt => GameEntities.Register(evt.Registry));
            bus.Subscribe<RegistryBuildingEvent<Item>>(evt => GameItems.Register(evt.Registry));
            bus.Subscribe<RegistryBuildingEvent<ICraftingRecipe>>(evt => GameRecipes.Register(evt.Registry));
            bus.Subscribe<RegistryBuildingEvent<IWorldgenAttribute>>(evt => WorldgenAttributes.Register(evt.Registry));
            bus.Subscribe<RegistryBuildingEvent<IWorldgenFeature>>(evt => WorldgenFeatures.Register(evt.Registry));
            bus.Subscribe<RegistryBuildingEvent<IParticleSystemData>>(ParticleSystems.Register);
        }
    }
}