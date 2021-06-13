using DigBuild.Engine.Items;
using DigBuild.Items;
using DigBuild.Registries;

namespace DigBuild.Behaviors
{
    public sealed class EquippableBehavior : IItemBehavior
    {
        public EquippableFlags Flags { get; set; }

        public EquippableBehavior(EquippableFlags flags)
        {
            Flags = flags;
        }

        public void Build(ItemBehaviorBuilder<object, object> item)
        {
            item.Add(ItemAttributes.Equippable, (_, _, _) => Flags);
        }
    }
}