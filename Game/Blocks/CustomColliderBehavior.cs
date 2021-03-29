using DigBuild.Engine.Blocks;
using DigBuild.Engine.Physics;

namespace DigBuild.Blocks
{
    public sealed class CustomColliderBehavior : IBlockBehavior
    {
        private readonly ICollider _collider;

        public CustomColliderBehavior(ICollider collider)
        {
            _collider = collider;
        }

        public void Build(BlockBehaviorBuilder<object, object> block)
        {
            block.Add(BlockAttributes.Collider, (_, _, _, _) => _collider);
        }
    }
}