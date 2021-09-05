using DigBuild.Engine.Items;
using DigBuild.Registries;

namespace DigBuild.Content.Behaviors
{
    public class MaxStackSizeBehavior : IItemBehavior
    {
        public static MaxStackSizeBehavior One { get; } = new(1);

        private readonly ushort _max;

        public MaxStackSizeBehavior(ushort max)
        {
            _max = max;
        }

        public void Build(ItemBehaviorBuilder<object, object> item)
        {
            item.Add(GameItemAttributes.MaxStackSize, (_, _, _) => _max);
        }
    }
}