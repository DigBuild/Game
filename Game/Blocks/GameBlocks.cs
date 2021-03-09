using DigBuild.Engine.Blocks;
using DigBuild.Engine.Math;
using DigBuild.Engine.Reg;
using DigBuild.Platform.Resource;

namespace DigBuild.Blocks
{
    public static class GameBlocks
    {
        public static Block Dirt { get; private set; } = null!;
        public static Block Grass { get; private set; } = null!;
        public static Block Water { get; private set; } = null!;
        public static Block Stone { get; private set; } = null!;

        public static Block TriangleBlock { get; private set; } = null!;

        internal static void Register(RegistryBuilder<Block> registry)
        {
            Dirt = registry.Create(new ResourceName(Game.Domain, "dirt"));
            Grass = registry.Create(new ResourceName(Game.Domain, "grass"), builder =>
            {
                builder.Attach(new FaceCoveredReplaceBehavior(BlockFace.PosY, () => Dirt));
            });
            Water = registry.Create(new ResourceName(Game.Domain, "water"), builder =>
            {
                builder.Attach(new NoPunchBehavior());
            });
            Stone = registry.Create(new ResourceName(Game.Domain, "stone"));
            
            TriangleBlock = registry.Create(new ResourceName(Game.Domain, "triangle_block"), builder =>
            {
                var data = builder.Add<CountingBlockData>();
                builder.Attach(new CountingBehavior(), data);
            });
        }
        
        private sealed class CountingBlockData : ICountingBehavior
        {
            public int Number { get; set; }
        }
    }
}