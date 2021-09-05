using DigBuild.Engine.Items;
using DigBuild.Engine.Registries;
using DigBuild.Players;
using DigBuild.Worlds;

namespace DigBuild.Items
{
    /// <summary>
    /// The game's item events.
    /// </summary>
    public static class ItemEvent
    {
        internal static void Register(TypeRegistryBuilder<IItemEvent, ItemEventInfo> registry)
        {
            registry.Register((Activate _) => Activate.Result.Fail);
            registry.Register((Punch _) => Punch.Result.Fail);
            registry.Register((EquipmentActivate _) => { });
            registry.Register((Use _) => { });
        }
        
        /// <summary>
        /// Fired when an item is left-clicked.
        /// </summary>
        public sealed class Activate : ItemEventBase, IItemEvent<Activate.Result>
        {
            /// <summary>
            /// The player that activated the item.
            /// </summary>
            public IPlayer Player { get; }
            /// <summary>
            /// The raycast hit in the world.
            /// </summary>
            public WorldRayCastContext.Hit? Hit { get; }

            public Activate(ItemInstance instance, IPlayer player, WorldRayCastContext.Hit? hit) : base(instance)
            {
                Player = player;
                Hit = hit;
            }
            
            /// <summary>
            /// The result state of the event.
            /// </summary>
            public enum Result
            {
                Success, Fail
            }
        }

        /// <summary>
        /// Fired when an item is right-clicked.
        /// </summary>
        public sealed class Punch : ItemEventBase, IItemEvent<Punch.Result>
        {
            /// <summary>
            /// The player that punched with the item.
            /// </summary>
            public IPlayer Player { get; }
            /// <summary>
            /// The raycast hit in the world.
            /// </summary>
            public WorldRayCastContext.Hit? Hit { get; }

            public Punch(ItemInstance instance, IPlayer player, WorldRayCastContext.Hit? hit) : base(instance)
            {
                Player = player;
                Hit = hit;
            }
            
            /// <summary>
            /// The result state of the event.
            /// </summary>
            public enum Result
            {
                Success, Fail
            }
        }

        /// <summary>
        /// Fired when the item is equipped and the corresponding key is pressed.
        /// </summary>
        public sealed class EquipmentActivate : ItemEventBase
        {
            /// <summary>
            /// The player that activated the item.
            /// </summary>
            public IPlayer Player { get; }

            public EquipmentActivate(ItemInstance instance, IPlayer player) : base(instance)
            {
                Player = player;
            }
        }
        
        /// <summary>
        /// Fired when the item is used inside the inventory.
        /// </summary>
        public sealed class Use : ItemEventBase
        {
            /// <summary>
            /// The player that used the item.
            /// </summary>
            public IPlayer Player { get; }

            public Use(ItemInstance instance, IPlayer player) : base(instance)
            {
                Player = player;
            }
        }
    }
    
    /// <summary>
    /// Registration/subscription extensions for item events.
    /// </summary>
    public static class ItemEventExtensions
    {
        /// <summary>
        /// Subscribes to the activated event.
        /// </summary>
        /// <typeparam name="TReadOnlyData">The read-only data type</typeparam>
        /// <typeparam name="TData">The read-write data type</typeparam>
        /// <param name="builder">The builder</param>
        /// <param name="onActivate">The handler</param>
        public static void Subscribe<TReadOnlyData, TData>(
            this IItemBehaviorBuilder<TReadOnlyData, TData> builder,
            ItemEventDelegate<TData, ItemEvent.Activate, ItemEvent.Activate.Result> onActivate
        )
            where TData : TReadOnlyData
        {
            builder.Subscribe(onActivate);
        }
        
        /// <summary>
        /// Fires the activated event.
        /// </summary>
        /// <param name="instance">The item instance</param>
        /// <param name="hit">The raycast hit</param>
        /// <param name="player">The player</param>
        public static ItemEvent.Activate.Result OnActivate(this ItemInstance instance, IPlayer player, WorldRayCastContext.Hit? hit)
        {
            return instance.Type.Post<ItemEvent.Activate, ItemEvent.Activate.Result>(new ItemEvent.Activate(instance, player, hit));
        }
        
        /// <summary>
        /// Subscribes to the punched event.
        /// </summary>
        /// <typeparam name="TReadOnlyData">The read-only data type</typeparam>
        /// <typeparam name="TData">The read-write data type</typeparam>
        /// <param name="builder">The builder</param>
        /// <param name="onPunch">The handler</param>
        public static void Subscribe<TReadOnlyData, TData>(
            this IItemBehaviorBuilder<TReadOnlyData, TData> builder,
            ItemEventDelegate<TData, ItemEvent.Punch, ItemEvent.Punch.Result> onPunch
        )
            where TData : TReadOnlyData
        {
            builder.Subscribe(onPunch);
        }
        
        /// <summary>
        /// Fires the punched event.
        /// </summary>
        /// <param name="instance">The item instance</param>
        /// <param name="hit">The raycast hit</param>
        /// <param name="player">The player</param>
        public static ItemEvent.Punch.Result OnPunch(this ItemInstance instance, IPlayer player, WorldRayCastContext.Hit? hit)
        {
            return instance.Type.Post<ItemEvent.Punch, ItemEvent.Punch.Result>(new ItemEvent.Punch(instance, player, hit));
        }
        
        /// <summary>
        /// Subscribes to the equipment activated event.
        /// </summary>
        /// <typeparam name="TReadOnlyData">The read-only data type</typeparam>
        /// <typeparam name="TData">The read-write data type</typeparam>
        /// <param name="builder">The builder</param>
        /// <param name="onEquipmentActivate">The handler</param>
        public static void Subscribe<TReadOnlyData, TData>(
            this IItemBehaviorBuilder<TReadOnlyData, TData> builder,
            ItemEventDelegate<TData, ItemEvent.EquipmentActivate> onEquipmentActivate
        )
            where TData : TReadOnlyData
        {
            builder.Subscribe(onEquipmentActivate);
        }
        
        /// <summary>
        /// Fires the equipment activated event.
        /// </summary>
        /// <param name="instance">The item instance</param>
        /// <param name="player">The player</param>
        public static void OnEquipmentActivate(this ItemInstance instance, IPlayer player)
        {
            instance.Type.Post(new ItemEvent.EquipmentActivate(instance, player));
        }
        
        /// <summary>
        /// Subscribes to the used event.
        /// </summary>
        /// <typeparam name="TReadOnlyData">The read-only data type</typeparam>
        /// <typeparam name="TData">The read-write data type</typeparam>
        /// <param name="builder">The builder</param>
        /// <param name="onUse">The handler</param>
        public static void Subscribe<TReadOnlyData, TData>(
            this IItemBehaviorBuilder<TReadOnlyData, TData> builder,
            ItemEventDelegate<TData, ItemEvent.Use> onUse
        )
            where TData : TReadOnlyData
        {
            builder.Subscribe(onUse);
        }
        
        /// <summary>
        /// Fires the used event.
        /// </summary>
        /// <param name="instance">The item instance</param>
        /// <param name="player">The player</param>
        public static void OnUse(this ItemInstance instance, IPlayer player)
        {
            instance.Type.Post(new ItemEvent.Use(instance, player));
        }
    }
}