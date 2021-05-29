using System;
using System.Collections.Generic;
using DigBuild.Blocks;
using DigBuild.Controller;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Impl.Worlds;
using DigBuild.Engine.Math;

namespace DigBuild.Content.Behaviors
{
    public sealed class VerticalSupportBehavior : IBlockBehavior
    {
        public void Build(BlockBehaviorBuilder<object, object> block)
        {
            block.Subscribe(OnNeighborChanged);
        }

        private void OnNeighborChanged(BlockEvent.NeighborChanged evt, object data, Action next)
        {
            var hasSupport = evt.World.GetBlock(evt.Pos.Offset(Direction.NegY)) != null;
            if (!hasSupport)
            {
                evt.World.TickScheduler.After(2).Tick += () =>
                {
                    if (evt.World.GetBlock(evt.Pos) == evt.Block)
                    {
                        evt.Block.OnBreaking(evt.World, evt.Pos);
                        evt.World.SetBlock(evt.Pos, null);

                        if (DigBuildGame.Instance.Controller is GameplayController controller)
                        {
                            controller.Boop();
                        }
                    }
                };
            }

            next();
        }
    }
}