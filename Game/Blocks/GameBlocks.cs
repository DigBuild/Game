using System;
using DigBuild.Behaviors;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Items;
using DigBuild.Engine.Math;
using DigBuild.Engine.Physics;
using DigBuild.Engine.Registries;
using DigBuild.Items;
using DigBuild.Platform.Resource;

namespace DigBuild.Blocks
{
    public static class GameBlocks
    {
        public static Block Dirt { get; private set; } = null!;
        public static Block Grass { get; private set; } = null!;
        public static Block Water { get; private set; } = null!;
        public static Block Stone { get; private set; } = null!;
        public static Block Crafter { get; private set; } = null!;

        internal static void Register(RegistryBuilder<Block> registry)
        {
            Dirt = registry.Create(new ResourceName(Game.Domain, "dirt"),
                BlockDrops(() => GameItems.Dirt)
            );
            Grass = registry.Create(new ResourceName(Game.Domain, "grass"), builder =>
                {
                    builder.Attach(new ReplaceOnFaceCoveredBehavior(Direction.PosY, () => Dirt));
                },
                BlockDrops(() => GameItems.Grass)
            );
            Water = registry.Create(new ResourceName(Game.Domain, "water"), builder =>
                {
                    builder.Attach(new ColliderBehavior(ICollider.None));
                    builder.Attach(new RayColliderBehavior(IRayCollider<VoxelRayCollider.Hit>.None));
                    builder.Attach(new NoPunchBehavior());
                },
                BlockDrops(() => GameItems.Water)
            );
            Stone = registry.Create(new ResourceName(Game.Domain, "stone"),
                BlockDrops(() => GameItems.Stone)
            );
            
            Crafter = registry.Create(new ResourceName(Game.Domain, "crafter"), builder =>
                {
                    var data = builder.Add<CrafterBlockData>();
                    builder.Attach(new FindCraftingRecipeBehavior(), data);
                    builder.Attach(new CraftingUiBehavior(), data);
                },
                BlockDrops(() => GameItems.Crafter)
            );
        }

        private static Action<BlockBuilder> BlockDrops(Func<Item> itemSupplier, ushort amount = 1)
        {
            return builder =>
            {
                builder.Attach(new DropItemBehavior(() => new ItemInstance(itemSupplier(), amount)));
            };
        }
    }
}