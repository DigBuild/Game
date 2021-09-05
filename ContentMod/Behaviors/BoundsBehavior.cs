using DigBuild.Engine.Blocks;
using DigBuild.Engine.Math;
using DigBuild.Engine.Physics;
using DigBuild.Registries;

namespace DigBuild.Content.Behaviors
{
    public sealed class BoundsBehavior : IBlockBehavior
    {
        private readonly AABB? _bounds;
        private readonly ICollider _collider;
        private readonly IRayCollider<VoxelRayCollider.Hit> _rayCollider;

        public BoundsBehavior(AABB? bounds)
        {
            _bounds = bounds;
            _collider = bounds.HasValue ? new VoxelCollider(bounds.Value) : ICollider.None;
            _rayCollider = bounds.HasValue ? new VoxelRayCollider(bounds.Value) : IRayCollider<VoxelRayCollider.Hit>.None;
        }

        public void Build(BlockBehaviorBuilder<object, object> block)
        {
            block.Add(GameBlockAttributes.Bounds, (_, _, _) => _bounds);
            block.Add(GameBlockAttributes.Collider, (_, _, _) => _collider);
            block.Add(GameBlockAttributes.RayCollider, (_, _, _) => _rayCollider);
        }
    }
}