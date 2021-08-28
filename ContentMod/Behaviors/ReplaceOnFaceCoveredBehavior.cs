using System;
using DigBuild.Blocks;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Math;
using DigBuild.Engine.Worlds.Impl;

namespace DigBuild.Content.Behaviors
{
    public sealed class ReplaceOnFaceCoveredBehavior : IBlockBehavior
    {
        private readonly Direction _face;
        private readonly Func<Block> _replacementSupplier;

        public ReplaceOnFaceCoveredBehavior(Direction face, Func<Block> replacementSupplier)
        {
            _face = face;
            _replacementSupplier = replacementSupplier;
        }

        public void Build(BlockBehaviorBuilder<object, object> block)
        {
            block.Subscribe(OnNeighborChanged);
        }

        private void OnNeighborChanged(BlockEvent.NeighborChanged evt, object data, Action next)
        {
            if (evt.Direction == _face)
            {
                var pos = evt.Pos.Offset(_face);
                var block = evt.World.GetBlock(pos);
                if (block != null && block
                    .Get(evt.World, pos, BlockFaceSolidity.Attribute).Solid
                    .Has(_face.GetOpposite()))
                {
                    evt.World.SetBlock(evt.Pos, _replacementSupplier());
                    return;
                }
            }
            next();
        }
    }
}