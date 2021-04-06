using DigBuild.Engine.Items;
using DigBuild.Engine.Registries;
using DigBuild.Players;
using DigBuild.Worlds;

namespace DigBuild.Items
{
    public static class ItemEvent
    {
        public static void Register(ExtendedTypeRegistryBuilder<IItemEvent, ItemEventInfo> registry)
        {
            registry.Register((IItemContext _, Activate _) => Activate.Result.Fail);
            registry.Register((IItemContext _, Punch _) => Punch.Result.Fail);
        }

        public sealed class Activate : IItemEvent<Activate.Result>
        {
            public readonly IPlayer Player;
            public readonly WorldRayCastContext.Hit? Hit;

            public Activate(IPlayer player, WorldRayCastContext.Hit? hit)
            {
                Player = player;
                Hit = hit;
            }

            public enum Result
            {
                Success, Fail
            }
        }

        public sealed class Punch : IItemEvent<Punch.Result>
        {
            public readonly IPlayer Player;
            public readonly WorldRayCastContext.Hit? Hit;

            public Punch(IPlayer player, WorldRayCastContext.Hit? hit)
            {
                Player = player;
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
        public static void Subscribe<TReadOnlyData, TData>(
            this IItemBehaviorBuilder<TReadOnlyData, TData> builder,
            ItemEventDelegate<TData, ItemEvent.Activate, ItemEvent.Activate.Result> onActivate
        )
            where TData : TReadOnlyData
        {
            builder.Subscribe(onActivate);
        }

        public static ItemEvent.Activate.Result OnActivate(this IItem item, IItemContext context, ItemEvent.Activate evt)
        {
            return item.Post<ItemEvent.Activate, ItemEvent.Activate.Result>(context, evt);
        }

        public static void Subscribe<TReadOnlyData, TData>(
            this IItemBehaviorBuilder<TReadOnlyData, TData> builder,
            ItemEventDelegate<TData, ItemEvent.Punch, ItemEvent.Punch.Result> onPunch
        )
            where TData : TReadOnlyData
        {
            builder.Subscribe(onPunch);
        }

        public static ItemEvent.Punch.Result OnPunch(this IItem item, IItemContext context, ItemEvent.Punch evt)
        {
            return item.Post<ItemEvent.Punch, ItemEvent.Punch.Result>(context, evt);
        }
    }
}