using DigBuild.Engine.Blocks;
using DigBuild.Engine.Physics;
using DigBuild.Registries;

namespace DigBuild.Content.Behaviors
{
    public sealed class RayColliderBehavior : IBlockBehavior
    {
        private readonly IRayCollider<VoxelRayCollider.Hit> _collider;

        public RayColliderBehavior(IRayCollider<VoxelRayCollider.Hit> collider)
        {
            _collider = collider;
        }

        public void Build(BlockBehaviorBuilder<object, object> block)
        {
            block.Add(BlockAttributes.RayCollider, (_, _, _) => _collider);
        }
    }
}