using DigBuild.Blocks;
using DigBuild.Engine.Blocks;
using DigBuild.Registries;

namespace DigBuild.Content.Behaviors
{
    public sealed class LightEmittingBehavior : IBlockBehavior
    {
        public byte Value { get; set; }

        public LightEmittingBehavior(byte value)
        {
            Value = value;
        }

        public void Build(BlockBehaviorBuilder<object, object> block)
        {
            block.Add(GameBlockAttributes.LightEmission, (_, _, _) => new LightEmission(Value));
        }
    }
}