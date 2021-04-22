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
            registry.Register((Activate _) => Activate.Result.Fail);
            registry.Register((Punch _) => Punch.Result.Fail);
        }

        public sealed class Activate : ItemEventBase, IItemEvent<Activate.Result>
        {
            public readonly IPlayer Player;
            public readonly WorldRayCastContext.Hit? Hit;

            public Activate(ItemInstance instance, IPlayer player, WorldRayCastContext.Hit? hit) : base(instance)
            {
                Player = player;
                Hit = hit;
            }

            public enum Result
            {
                Success, Fail
            }
        }

        public sealed class Punch : ItemEventBase, IItemEvent<Punch.Result>
        {
            public readonly IPlayer Player;
            public readonly WorldRayCastContext.Hit? Hit;

            public Punch(ItemInstance instance, IPlayer player, WorldRayCastContext.Hit? hit) : base(instance)
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

        public static ItemEvent.Activate.Result OnActivate(this Item item, ItemInstance instance, IPlayer player, WorldRayCastContext.Hit? hit)
        {
            return item.Post<ItemEvent.Activate, ItemEvent.Activate.Result>(new ItemEvent.Activate(instance, player, hit));
        }

        public static ItemEvent.Activate.Result OnActivate(this ItemInstance instance, IPlayer player, WorldRayCastContext.Hit? hit)
        {
            return instance.Type.OnActivate(instance, player, hit);
        }

        public static void Subscribe<TReadOnlyData, TData>(
            this IItemBehaviorBuilder<TReadOnlyData, TData> builder,
            ItemEventDelegate<TData, ItemEvent.Punch, ItemEvent.Punch.Result> onPunch
        )
            where TData : TReadOnlyData
        {
            builder.Subscribe(onPunch);
        }

        public static ItemEvent.Punch.Result OnPunch(this Item item, ItemInstance instance, IPlayer player, WorldRayCastContext.Hit? hit)
        {
            return item.Post<ItemEvent.Punch, ItemEvent.Punch.Result>(new ItemEvent.Punch(instance, player, hit));
        }

        public static ItemEvent.Punch.Result OnPunch(this ItemInstance instance, IPlayer player, WorldRayCastContext.Hit? hit)
        {
            return instance.Type.OnPunch(instance, player, hit);
        }
    }
}