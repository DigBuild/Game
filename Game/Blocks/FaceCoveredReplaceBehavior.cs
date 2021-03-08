using System;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Math;
using DigBuild.Engine.Voxel;

namespace DigBuild.Blocks
{
    public sealed class FaceCoveredReplaceBehavior : IBlockBehavior<object>
    {
        private readonly BlockFace _face;
        private readonly Func<Block> _replacementSupplier;

        public FaceCoveredReplaceBehavior(BlockFace face, Func<Block> replacementSupplier)
        {
            _face = face;
            _replacementSupplier = replacementSupplier;
        }

        public void Build(BlockBehaviorBuilder<object> block)
        {
            block.Subscribe(OnNeighborChanged);
        }

        private void OnNeighborChanged(IBlockContext context, object data, BlockEvent.NeighborChanged evt, Action next)
        {
            if (evt.Direction == _face && context.World.GetBlock(context.Pos.Offset(_face)) != null)
                context.World.SetBlock(context.Pos, _replacementSupplier());
        }
    }
}