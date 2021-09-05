using System;
using System.Collections.Generic;
using DigBuild.Behaviors;
using DigBuild.Content.Behaviors;
using DigBuild.Content.Data;
using DigBuild.Content.Worldgen.Structure;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Items;
using DigBuild.Engine.Math;
using DigBuild.Engine.Registries;
using DigBuild.Items;
using DigBuild.Platform.Resource;
using DigBuild.Render.Models.Json;

namespace DigBuild.Content.Registries
{
    public static class GameItems
    {
        public static Item Dirt { get; private set; } = null!;
        public static Item Grass { get; private set; } = null!;
        public static Item Sand { get; private set; } = null!;
        public static Item Stone { get; private set; } = null!;
        public static Item Gravel { get; private set; } = null!;
        public static Item Ice { get; private set; } = null!;
        public static Item Snow { get; private set; } = null!;
        public static Item Log { get; private set; } = null!;
        public static Item LogSmall { get; private set; } = null!;
        public static Item Leaves { get; private set; } = null!;
        public static Item StoneStairs { get; private set; } = null!;
        public static Item Crafter { get; private set; } = null!;
        
        public static Item Campfire { get; private set; } = null!;
        
        public static Item Sapling { get; private set; } = null!;
        public static Item Twig { get; private set; } = null!;

        public static Item Tallgrass { get; private set; } = null!;
        public static Item Barley { get; private set; } = null!;

        public static Item Pouch { get; private set; } = null!;

        internal static void Register(RegistryBuilder<Item> registry)
        {
            Dirt = registry.Register(new ResourceName(DigBuildGame.Domain, "dirt"),
                BlockPlacement(() => GameBlocks.Dirt)
            );
            Grass = registry.Register(new ResourceName(DigBuildGame.Domain, "grass"),
                BlockPlacement(() => GameBlocks.Grass)
            );
            Sand = registry.Register(new ResourceName(DigBuildGame.Domain, "sand"),
                BlockPlacement(() => GameBlocks.Sand)
            );
            Stone = registry.Register(new ResourceName(DigBuildGame.Domain, "stone"),
                BlockPlacement(() => GameBlocks.Stone)
            );
            Gravel = registry.Register(new ResourceName(DigBuildGame.Domain, "gravel"),
                BlockPlacement(() => GameBlocks.Gravel)
            );
            Ice = registry.Register(new ResourceName(DigBuildGame.Domain, "ice"),
                BlockPlacement(() => GameBlocks.Ice)
            );
            Snow = registry.Register(new ResourceName(DigBuildGame.Domain, "snow"),
                BlockPlacement(() => GameBlocks.Snow)
            );
            Log = registry.Register(new ResourceName(DigBuildGame.Domain, "log"),
                BlockPlacement(() => GameBlocks.Log)
            );
            LogSmall = registry.Register(new ResourceName(DigBuildGame.Domain, "log_small"),
                BlockPlacement(() => GameBlocks.LogSmall)
            );
            Leaves = registry.Register(new ResourceName(DigBuildGame.Domain, "leaves"),
                BlockPlacement(() => GameBlocks.Leaves)
            );
            StoneStairs = registry.Register(new ResourceName(DigBuildGame.Domain, "stone_stairs"),
                BlockPlacement(() => GameBlocks.StoneStairs)
            );
            Crafter = registry.Register(new ResourceName(DigBuildGame.Domain, "crafter"),
                BlockPlacement(() => GameBlocks.Crafter)
            );

            Campfire = registry.Register(new ResourceName(DigBuildGame.Domain, "campfire"),
                BlockPlacement(() => GameBlocks.Campfire)
            );
            
            Sapling = registry.Register(new ResourceName(DigBuildGame.Domain, "sapling"), builder =>
            {
                builder.Attach(new PlaceMultiblockBehavior(() => TreeStructure.Blocks));
            });
            Twig = registry.Register(new ResourceName(DigBuildGame.Domain, "twig"));
            
            Tallgrass = registry.Register(new ResourceName(DigBuildGame.Domain, "tallgrass"), 
                BlockPlacement(() => GameBlocks.Tallgrass)
            );
            Barley = registry.Register(new ResourceName(DigBuildGame.Domain, "barley"), 
                BlockPlacement(() => GameBlocks.Barley)
            );
            
            Pouch = registry.Register(new ResourceName(DigBuildGame.Domain, "pouch"), builder =>
            {
                var data = builder.Add<PouchData>();
                builder.Attach(new PouchBehavior(), data);

                builder.Attach(new EquippableBehavior(EquippableFlags.Equipment));
                builder.Attach(MaxStackSizeBehavior.One);
            });
        }

        private static Action<ItemBuilder> BlockPlacement(Func<Block> blockSupplier)
        {
            return builder => builder.Attach(new PlaceBlockBehavior(blockSupplier));
        }
    }
}