using System;
using DigBuild.Blocks;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Impl.Worlds;
using DigBuild.Engine.Math;
using DigBuild.Engine.Worlds;

namespace DigBuild.Behaviors
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
            if (evt.Direction == _face && evt.World.GetBlock(evt.Pos.Offset(_face)) != null)
            {
                evt.World.SetBlock(evt.Pos, _replacementSupplier());
            }
            else
            {
                next();
            }
        }
    }
}