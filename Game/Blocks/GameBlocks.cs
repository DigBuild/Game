using DigBuild.Engine.Blocks;
using DigBuild.Engine.Reg;
using DigBuild.Platform.Resource;

namespace DigBuild.Blocks
{
    public static class GameBlocks
    {
        public static Block Terrain { get; private set; } = null!;
        public static Block Water { get; private set; } = null!;

        // public static Block CountingBlock { get; private set; } = null!;

        internal static void Register(RegistryBuilder<Block> registry)
        {
            Terrain = registry.Create(new ResourceName(Game.Domain, "terrain"));
            Water = registry.Create(new ResourceName(Game.Domain, "water"));
            
            // CountingBlock = registry.Create(new ResourceName(Game.Domain, "counting_block"), builder =>
            // {
            //     var data = builder.Add<CountingBlockData>();
            //     builder.Attach(new CountingBehavior(), data);
            // });
        }
        
        // private sealed class CountingBlockData : ICountingBehavior
        // {
        //     public int Number { get; set; }
        // }
    }
}