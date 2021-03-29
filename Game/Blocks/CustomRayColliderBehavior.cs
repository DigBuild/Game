using DigBuild.Engine.Blocks;
using DigBuild.Engine.Physics;

namespace DigBuild.Blocks
{
    public sealed class CustomRayColliderBehavior : IBlockBehavior
    {
        private readonly IRayCollider<VoxelRayCollider.Hit> _collider;

        public CustomRayColliderBehavior(IRayCollider<VoxelRayCollider.Hit> collider)
        {
            _collider = collider;
        }

        public void Build(BlockBehaviorBuilder<object, object> block)
        {
            block.Add(BlockAttributes.RayCollider, (_, _, _, _) => _collider);
        }
    }
}