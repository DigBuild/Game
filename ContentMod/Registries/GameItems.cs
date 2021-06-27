using System;
using System.Collections.Generic;
using DigBuild.Behaviors;
using DigBuild.Content.Worldgen.Structure;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Items;
using DigBuild.Engine.Math;
using DigBuild.Engine.Registries;
using DigBuild.Platform.Resource;

namespace DigBuild.Content.Registries
{
    public static class GameItems
    {
        public static Item Dirt { get; private set; } = null!;
        public static Item Grass { get; private set; } = null!;
        public static Item Sand { get; private set; } = null!;
        public static Item Stone { get; private set; } = null!;
        public static Item Log { get; private set; } = null!;
        public static Item LogSmall { get; private set; } = null!;
        public static Item Leaves { get; private set; } = null!;
        public static Item StoneStairs { get; private set; } = null!;
        public static Item Crafter { get; private set; } = null!;
        
        public static Item Campfire { get; private set; } = null!;
        
        public static Item Sapling { get; private set; } = null!;
        public static Item Twig { get; private set; } = null!;

        public static Item Multiblock { get; private set; } = null!;

        internal static void Register(RegistryBuilder<Item> registry)
        {
            Dirt = registry.Create(new ResourceName(DigBuildGame.Domain, "dirt"),
                BlockPlacement(() => GameBlocks.Dirt)
            );
            Grass = registry.Create(new ResourceName(DigBuildGame.Domain, "grass"),
                BlockPlacement(() => GameBlocks.Grass)
            );
            Sand = registry.Create(new ResourceName(DigBuildGame.Domain, "sand"),
                BlockPlacement(() => GameBlocks.Sand)
            );
            Stone = registry.Create(new ResourceName(DigBuildGame.Domain, "stone"),
                BlockPlacement(() => GameBlocks.Stone)
            );
            Log = registry.Create(new ResourceName(DigBuildGame.Domain, "log"),
                BlockPlacement(() => GameBlocks.Log)
            );
            LogSmall = registry.Create(new ResourceName(DigBuildGame.Domain, "log_small"),
                BlockPlacement(() => GameBlocks.LogSmall)
            );
            Leaves = registry.Create(new ResourceName(DigBuildGame.Domain, "leaves"),
                BlockPlacement(() => GameBlocks.Leaves)
            );
            StoneStairs = registry.Create(new ResourceName(DigBuildGame.Domain, "stone_stairs"),
                BlockPlacement(() => GameBlocks.StoneStairs)
            );
            Crafter = registry.Create(new ResourceName(DigBuildGame.Domain, "crafter"),
                BlockPlacement(() => GameBlocks.Crafter)
            );

            Campfire = registry.Create(new ResourceName(DigBuildGame.Domain, "campfire"),
                BlockPlacement(() => GameBlocks.Campfire)
            );
            
            Sapling = registry.Create(new ResourceName(DigBuildGame.Domain, "sapling"), builder =>
            {
                builder.Attach(new PlaceMultiblockBehavior(() => TreeStructure.Blocks));
            });
            Twig = registry.Create(new ResourceName(DigBuildGame.Domain, "twig"));
            
            Multiblock = registry.Create(new ResourceName(DigBuildGame.Domain, "multiblock"), builder =>
            {
                builder.Attach(new PlaceMultiblockBehavior(() => new Dictionary<Vector3I, Block>()
                {
                    [new Vector3I(0, 0, 0)] = GameBlocks.Multiblock,
                    [new Vector3I(0, 0, 1)] = GameBlocks.Multiblock,
                    [new Vector3I(0, 1, 1)] = GameBlocks.Multiblock,
                }));
            });
        }

        private static Action<ItemBuilder> BlockPlacement(Func<Block> blockSupplier)
        {
            return builder => builder.Attach(new PlaceBlockBehavior(blockSupplier));
        }
    }
}