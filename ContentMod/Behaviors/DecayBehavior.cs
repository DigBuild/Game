using System;
using System.Collections.Generic;
using DigBuild.Blocks;
using DigBuild.Controller;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Impl.Worlds;
using DigBuild.Engine.Math;

namespace DigBuild.Content.Behaviors
{
    public sealed class DecayBehavior : IBlockBehavior
    {
        public uint LookupRadiusSquared { get; }
        public Func<Block, bool> IsSupportBlock { get; }
        public Func<Block, bool> IsTransferBlock { get; }

        public DecayBehavior(uint lookupRadius, Func<Block, bool> isSupportBlock, Func<Block, bool> isTransferBlock)
        {
            LookupRadiusSquared = lookupRadius * lookupRadius;
            IsSupportBlock = isSupportBlock;
            IsTransferBlock = isTransferBlock;
        }

        public void Build(BlockBehaviorBuilder<object, object> block)
        {
            block.Subscribe(OnNeighborChanged);
        }

        private void OnNeighborChanged(BlockEvent.NeighborChanged evt, object data, Action next)
        {
            var visited = new HashSet<BlockPos> {evt.Pos};
            var toVisit = new Queue<BlockPos>();
            toVisit.Enqueue(evt.Pos);

            var hasSupport = false;

            while (toVisit.Count > 0)
            {
                var pos = toVisit.Dequeue();
                var block = evt.World.GetBlock(pos);
                if (block == null)
                    continue;

                if (IsSupportBlock(block))
                {
                    hasSupport = true;
                    break;
                }

                if (IsTransferBlock(block))
                {
                    foreach (var direction in Directions.All)
                    {
                        var nextPos = pos.Offset(direction);
                        if (new Vector3I(nextPos - evt.Pos).LengthSquared() <= LookupRadiusSquared && visited.Add(nextPos))
                            toVisit.Enqueue(nextPos);
                    }
                }
            }

            if (!hasSupport)
            {
                evt.World.TickScheduler.After((ulong) new Random().Next(5, 8)).Tick += () =>
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