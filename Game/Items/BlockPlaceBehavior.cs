﻿using System;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Items;
using DigBuild.Engine.Voxel;

namespace DigBuild.Items
{
    public sealed class BlockPlaceBehavior : IItemBehavior<object>
    {
        private readonly Func<Block> _blockSupplier;

        public BlockPlaceBehavior(Func<Block> blockSupplier)
        {
            _blockSupplier = blockSupplier;
        }

        public void Build(ItemBehaviorBuilder<object> item)
        {
            item.Subscribe(OnActivate);
        }

        private ItemEvent.Activate.Result OnActivate(IPlayerItemContext context, object data, ItemEvent.Activate evt, Func<ItemEvent.Activate.Result> next)
        {
            if (evt.Hit == null)
                return next();

            if (context.World.SetBlock(evt.Hit.BlockPos.Offset(evt.Hit.Face), _blockSupplier()))
            {
                context.Instance.Count--;
                Console.WriteLine($"New item count is {context.Instance.Count}");
                return ItemEvent.Activate.Result.Success;
            }

            return ItemEvent.Activate.Result.Fail;
        }
    }
}