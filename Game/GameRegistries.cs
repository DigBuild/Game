using DigBuild.Blocks;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Reg;
using DigBuild.Engine.Worldgen;
using DigBuild.Platform.Resource;

namespace DigBuild
{
    public static class GameRegistries
    {
        public static ExtendedTypeRegistry<IBlockEvent, BlockEventInfo> BlockEvents { get; private set; } = null!;
        
        public static Registry<IBlockAttribute> BlockAttributes { get; private set; } = null!;
        public static Registry<IBlockCapability> BlockCapabilities { get; private set; } = null!;

        public static Registry<Block> Blocks { get; private set; } = null!;
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