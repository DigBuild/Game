using System;
using DigBuild.Behaviors;
using DigBuild.Content.Behaviors;
using DigBuild.Content.Blocks;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Items;
using DigBuild.Engine.Math;
using DigBuild.Engine.Physics;
using DigBuild.Engine.Registries;
using DigBuild.Render.Models;

namespace DigBuild.Content.Registries
{
    public static class GameBlocks
    {
        public static readonly AABB[] StoneStairAABBs = {
            new(0, 0, 0, 1, 0.5f, 1),
            new(0.5f, 0.5f, 0, 1, 1, 1)
        };

        public static Block Dirt { get; private set; } = null!;
        public static Block Grass { get; private set; } = null!;
        public static Block Sand { get; private set; } = null!;
        public static Block Water { get; private set; } = null!;
        public static Block Stone { get; private set; } = null!;
        public static Block Log { get; private set; } = null!;
        public static Block LogSmall { get; private set; } = null!;
        public static Block Leaves { get; private set; } = null!;
        public static Block StoneStairs { get; private set; } = null!;
        public static Block Crafter { get; private set; } = null!;
        public static Block Campfire { get; private set; } = null!;
        
        public static Block Multiblock { get; private set; } = null!;
        
        internal static void Register(RegistryBuilder<Block> registry)
        {
            Dirt = registry.Create(DigBuildGame.Domain, "dirt",
                Drops(() => GameItems.Dirt)
            );
            Grass = registry.Create(DigBuildGame.Domain, "grass", builder =>
                {
                    builder.Attach(new ReplaceOnFaceCoveredBehavior(Direction.PosY, () => Dirt));
                },
                Drops(() => GameItems.Dirt)
            );
            Sand = registry.Create(DigBuildGame.Domain, "sand",
                Drops(() => GameItems.Sand)
            );
            Water = registry.Create(DigBuildGame.Domain, "water", builder =>
                {
                    builder.Attach(new ColliderBehavior(ICollider.None));
                    builder.Attach(new RayColliderBehavior(IRayCollider<VoxelRayCollider.Hit>.None));
                    builder.Attach(new NoPunchBehavior());
                    builder.Attach(new NonSolidBehavior());
                    builder.Attach(new WaterBehavior());
                }
            );
            Stone = registry.Create(DigBuildGame.Domain, "stone",
                Drops(() => GameItems.Stone)
            );
            Log = registry.Create(DigBuildGame.Domain, "log", builder =>
            {
                builder.Attach(new VerticalSupportBehavior());
            },
                Drops(() => GameItems.Log)
            );
            LogSmall = registry.Create(DigBuildGame.Domain, "log_small", builder =>
                {
                    var aabb = new AABB(0.25f, 0, 0.25f, 0.75f, 1, 0.75f);
                    builder.Attach(new ColliderBehavior(new VoxelCollider(aabb)));
                    builder.Attach(new RayColliderBehavior(new VoxelRayCollider(aabb)));
                    
                    builder.Attach(new VerticalSupportBehavior());
                    builder.Attach(new NonSolidBehavior());
                },
                Drops(() => GameItems.Log)
            );

            Leaves = registry.Create(DigBuildGame.Domain, "leaves", builder =>
            {
                builder.Attach(new DecayBehavior(
                    3,
                    block => block == Log || block == LogSmall,
                    block => block == Leaves
                ));

                builder.Attach(new NonSolidBehavior());
            },
                Drops(() => GameItems.Sapling, 1, 0.1f),
                Drops(() => GameItems.Twig, 1, 0.2f)
            );

            StoneStairs = registry.Create(DigBuildGame.Domain, "stone_stairs", builder =>
                {
                    builder.Attach(new ColliderBehavior(new MultiVoxelCollider(StoneStairAABBs)));
                    builder.Attach(new RayColliderBehavior(new VoxelRayCollider(StoneStairAABBs)));

                    var horizontalDirection = builder.Add<HorizontalPlacementData>();
                    builder.Attach(new HorizontalPlacementBehavior(), horizontalDirection);
                    builder.Attach(new CustomModelDataBehavior<HorizontalPlacementData, JsonModelData>((_, data, model) =>
                    {
                        model["direction"] = data.Direction.ToString().ToLowerInvariant();
                    }), horizontalDirection);

                    builder.Attach(new NonSolidBehavior());
                },
                Drops(() => GameItems.StoneStairs)
            );

            Crafter = registry.Create(DigBuildGame.Domain, "crafter", builder =>
                {
                    var data = builder.Add<CrafterBlockData>();
                    builder.Attach(new FindCraftingRecipeBehavior(), data);
                    builder.Attach(new CraftingUiBehavior(), data);

                    builder.Attach(new LightEmittingBehavior(0xF));
                    builder.Attach(new NonSolidBehavior());
                },
                Drops(() => GameItems.Crafter)
            );

            Campfire = registry.Create(DigBuildGame.Domain, "campfire", builder =>
                {
                    builder.Attach(new ColliderBehavior(ICollider.None));
                    builder.Attach(new RayColliderBehavior(new VoxelRayCollider(new AABB(0.125f, 0, 0.125f, 0.875f, 0.5f, 0.875f))));

                    builder.Attach(new LightEmittingBehavior(0x8));
                    builder.Attach(new NonSolidBehavior());

                    builder.Attach(new CampfireBehavior());
                },
                Drops(() => GameItems.Campfire)
            );
            

            Multiblock = registry.Create(DigBuildGame.Domain, "multiblock", builder =>
            {
                var data = builder.Add<MultiblockData>();
                builder.Attach(new MultiblockBehavior(), data);
            });
        }

        private static Action<BlockBuilder> Drops(Func<Item> itemSupplier, ushort amount = 1, float probability = 1)
        {
            return builder =>
            {
                builder.Attach(new DropItemBehavior(() => new ItemInstance(itemSupplier(), amount), probability));
            };
        }
    }
}