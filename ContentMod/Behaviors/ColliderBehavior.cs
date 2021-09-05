using DigBuild.Engine.Blocks;
using DigBuild.Engine.Physics;
using DigBuild.Registries;

namespace DigBuild.Content.Behaviors
{
    public sealed class ColliderBehavior : IBlockBehavior
    {
        private readonly ICollider _collider;

        public ColliderBehavior(ICollider collider)
        {
            _collider = collider;
        }

        public void Build(BlockBehaviorBuilder<object, object> block)
        {
            block.Add(GameBlockAttributes.Collider, (_, _, _) => _collider);
        }
    }
}