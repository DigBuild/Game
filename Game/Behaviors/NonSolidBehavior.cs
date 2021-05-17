using DigBuild.Engine.Blocks;

namespace DigBuild.Behaviors
{
    public sealed class NonSolidBehavior : IBlockBehavior
    {
        public void Build(BlockBehaviorBuilder<object, object> block)
        {
            block.Add(BlockFaceSolidity.Attribute, (_, _, _) => BlockFaceSolidity.None);
        }
    }
}