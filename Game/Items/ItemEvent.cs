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
            registry.Register((EquipmentActivate _) => { });
            registry.Register((Use _) => { });
        }

        public sealed class Activate : ItemEventBase, IItemEvent<Activate.Result>
        {
            public IPlayer Player { get; }
            public WorldRayCastContext.Hit? Hit { get; }

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
            public IPlayer Player { get; }
            public WorldRayCastContext.Hit? Hit { get; }

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

        public sealed class EquipmentActivate : ItemEventBase
        {
            public IPlayer Player { get; }

            public EquipmentActivate(ItemInstance instance, IPlayer player) : base(instance)
            {
                Player = player;
            }
        }

        public sealed class Use : ItemEventBase
        {
            public IPlayer Player { get; }

            public Use(ItemInstance instance, IPlayer player) : base(instance)
            {
                Player = player;
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

        public static void Subscribe<TReadOnlyData, TData>(
            this IItemBehaviorBuilder<TReadOnlyData, TData> builder,
            ItemEventDelegate<TData, ItemEvent.EquipmentActivate> onEquipmentActivate
        )
            where TData : TReadOnlyData
        {
            builder.Subscribe(onEquipmentActivate);
        }

        public static void OnEquipmentActivate(this Item item, ItemInstance instance, IPlayer player)
        {
            item.Post(new ItemEvent.EquipmentActivate(instance, player));
        }

        public static void OnEquipmentActivate(this ItemInstance instance, IPlayer player)
        {
            instance.Type.OnEquipmentActivate(instance, player);
        }

        public static void Subscribe<TReadOnlyData, TData>(
            this IItemBehaviorBuilder<TReadOnlyData, TData> builder,
            ItemEventDelegate<TData, ItemEvent.Use> onUse
        )
            where TData : TReadOnlyData
        {
            builder.Subscribe(onUse);
        }

        public static void OnUse(this Item item, ItemInstance instance, IPlayer player)
        {
            item.Post(new ItemEvent.Use(instance, player));
        }

        public static void OnUse(this ItemInstance instance, IPlayer player)
        {
            instance.Type.OnUse(instance, player);
        }
    }
}