using DigBuild.Blocks;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Physics;
using DigBuild.Registries;

namespace DigBuild.Behaviors
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
            block.Add(BlockAttributes.Collider, (_, _, _, _) => _collider);
        }
    }
}