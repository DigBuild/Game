using DigBuild.Engine.Items;
using DigBuild.Engine.Reg;
using DigBuild.Voxel;

namespace DigBuild.Items
{
    public static class ItemEvent
    {
        public static void Register(ExtendedTypeRegistryBuilder<IItemEvent, ItemEventInfo> registry)
        {
            registry.Register((IPlayerItemContext _, Activate _) => Activate.Result.Fail);
            registry.Register((IPlayerItemContext _, Punch _) => Punch.Result.Fail);
        }

        public sealed class Activate : IItemEvent<IPlayerItemContext, Activate.Result>
        {
            public readonly WorldRayCastContext.Hit? Hit;

            public Activate(WorldRayCastContext.Hit? hit)
            {
                Hit = hit;
            }

            public enum Result
            {
                Success, Fail
            }
        }

        public sealed class Punch : IItemEvent<IPlayerItemContext, Punch.Result>
        {
            public readonly WorldRayCastContext.Hit? Hit;

            public Punch(WorldRayCastContext.Hit? hit)
            {
                Hit = hit;
            }

            public enum Result
            {
                Success, Fail
            }
        }
    }

    public static class ItemEventExtensions
    {
        public static void Subscribe<TData>(
            this IItemBehaviorBuilder<TData> builder,
            ItemEventDelegate<IPlayerItemContext, TData, ItemEvent.Activate, ItemEvent.Activate.Result> onActivate
        )
        {
            builder.Subscribe(onActivate);
        }

        public static ItemEvent.Activate.Result OnActivate(this IItem item, IPlayerItemContext context, ItemEvent.Activate evt)
        {
            return item.Post<IPlayerItemContext, ItemEvent.Activate, ItemEvent.Activate.Result>(context, evt);
        }

        public static void Subscribe<TData>(
            this IItemBehaviorBuilder<TData> builder,
            ItemEventDelegate<IPlayerItemContext, TData, ItemEvent.Punch, ItemEvent.Punch.Result> onPunch
        )
        {
            builder.Subscribe(onPunch);
        }

        public static ItemEvent.Punch.Result OnPunch(this IItem item, IPlayerItemContext context, ItemEvent.Punch evt)
        {
            return item.Post<IPlayerItemContext, ItemEvent.Punch, ItemEvent.Punch.Result>(context, evt);
        }
    }
}