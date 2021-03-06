using DigBuild.Engine.Blocks;
using DigBuild.Engine.Math;

namespace DigBuild.Blocks
{
    public sealed class CustomColliderBehavior : IBlockBehavior<object>
    {
        private readonly ICollider _collider;

        public CustomColliderBehavior(ICollider collider)
        {
            _collider = collider;
        }

        public void Build(BlockBehaviorBuilder<object> block)
        {
            block.Add(BlockAttributes.Collider, (_, _, _, _) => _collider);
        }
    }
}