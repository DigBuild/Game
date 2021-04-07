﻿using System;
using DigBuild.Behaviors;
using DigBuild.Blocks;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Items;
using DigBuild.Engine.Math;
using DigBuild.Engine.Physics;
using DigBuild.Engine.Registries;
using DigBuild.Platform.Resource;

namespace DigBuild.Registries
{
    public static class GameBlocks
    {
        public static readonly AABB[] StoneStairAABBs = {
            new(0, 0, 0, 1, 0.5f, 1),
            new(0.5f, 0.5f, 0, 1, 1, 1)
        };

        public static Block Dirt { get; private set; } = null!;
        public static Block Grass { get; private set; } = null!;
        public static Block Water { get; private set; } = null!;
        public static Block Stone { get; private set; } = null!;
        public static Block StoneStairs { get; private set; } = null!;
        public static Block Crafter { get; private set; } = null!;

        public static Block Glowy { get; private set; } = null!;

        internal static void Register(RegistryBuilder<Block> registry)
        {
            Dirt = registry.Create(new ResourceName(Game.Domain, "dirt"),
                Drops(() => GameItems.Dirt)
            );
            Grass = registry.Create(new ResourceName(Game.Domain, "grass"), builder =>
                {
                    builder.Attach(new ReplaceOnFaceCoveredBehavior(Direction.PosY, () => Dirt));
                },
                Drops(() => GameItems.Dirt)
            );
            Water = registry.Create(new ResourceName(Game.Domain, "water"), builder =>
                {
                    builder.Attach(new ColliderBehavior(ICollider.None));
                    builder.Attach(new RayColliderBehavior(IRayCollider<VoxelRayCollider.Hit>.None));
                    builder.Attach(new NoPunchBehavior());
                },
                Drops(() => GameItems.Water)
            );
            Stone = registry.Create(new ResourceName(Game.Domain, "stone"),
                Drops(() => GameItems.Stone)
            );
            StoneStairs = registry.Create(new ResourceName(Game.Domain, "stone_stairs"), builder =>
                {
                    // builder.Attach(new ColliderBehavior(new VoxelCollider(StoneStairAABBs)));
                    builder.Attach(new RayColliderBehavior(new VoxelRayCollider(StoneStairAABBs)));
                },
                Drops(() => GameItems.StoneStairs)
            );
            Crafter = registry.Create(new ResourceName(Game.Domain, "crafter"), builder =>
                {
                    var data = builder.Add<CrafterBlockData>();
                    builder.Attach(new FindCraftingRecipeBehavior(), data);
                    builder.Attach(new CraftingUiBehavior(), data);

                    builder.Attach(new LightEmittingBehavior(0xF));
                },
                Drops(() => GameItems.Crafter)
            );

            Glowy = registry.Create(new ResourceName(Game.Domain, "glowy"),
                Drops(() => GameItems.Glowy)
            );
        }

        private static Action<BlockBuilder> Drops(Func<Item> itemSupplier, ushort amount = 1)
        {
            return builder =>
            {
                builder.Attach(new DropItemBehavior(() => new ItemInstance(itemSupplier(), amount)));
            };
        }
    }
}