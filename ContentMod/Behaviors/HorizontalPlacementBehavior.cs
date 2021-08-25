using System;
using System.Numerics;
using DigBuild.Blocks;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Math;
using DigBuild.Engine.Storage;
using DigBuild.Registries;

namespace DigBuild.Content.Behaviors
{
    public interface IHorizontalPlacementBehavior
    {
        Direction Direction { get; set; }
    }

    public class HorizontalPlacementBehavior : IBlockBehavior<IHorizontalPlacementBehavior>
    {
        public void Build(BlockBehaviorBuilder<IHorizontalPlacementBehavior, IHorizontalPlacementBehavior> block)
        {
            block.Add(BlockAttributes.HorizontalDirection, (_, data, _) => data.Direction);
            block.Add(BlockAttributes.Direction, (_, data, _) => data.Direction);
            block.Subscribe(OnPlaced);
        }

        private void OnPlaced(BlockEvent.Placed evt, IHorizontalPlacementBehavior data, Action next)
        {
            var forward = evt.Player.GetCamera(0).Forward;
            var xzForward = new Vector3(forward.X, 0, forward.Z);
            var direction = Directions.FromOffset(xzForward);
            data.Direction = direction;
        }
    }

    public sealed class HorizontalPlacementData : IData<HorizontalPlacementData>, IHorizontalPlacementBehavior
    {
        public Direction Direction { get; set; }

        public HorizontalPlacementData Copy()
        {
            return new()
            {
                Direction = Direction
            };
        }
    }
}