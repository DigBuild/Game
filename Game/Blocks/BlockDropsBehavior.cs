using System;
using System.Numerics;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Items;
using DigBuild.Engine.Math;
using DigBuild.Engine.Voxel;
using DigBuild.Entities;

namespace DigBuild.Blocks
{
    public sealed class BlockDropsBehavior : IBlockBehavior<object>
    {
        private readonly Func<ItemInstance> _dropSupplier;

        public BlockDropsBehavior(Func<ItemInstance> dropSupplier)
        {
            _dropSupplier = dropSupplier;
        }

        public void Build(BlockBehaviorBuilder<object> block)
        {
            block.Subscribe(OnBroken);
        }

        private void OnBroken(IBlockContext context, object data, BlockEvent.Broken evt, Action next)
        {
            context.World.AddEntity(GameEntities.Item)
                .WithPosition(((Vector3) context.Pos) + new Vector3(0.5f, 0, 0.5f))
                .WithItem(_dropSupplier());
            next();
        }
    }
}