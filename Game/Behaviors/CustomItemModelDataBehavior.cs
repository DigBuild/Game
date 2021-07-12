using System;
using DigBuild.Engine.Items;
using DigBuild.Engine.Render.Models;
using DigBuild.Engine.Storage;

namespace DigBuild.Behaviors
{
    public sealed class CustomItemModelDataBehavior<T> : IItemBehavior where T : notnull, new()
    {
        private readonly Action<IReadOnlyItemInstance, T> _action;

        public CustomItemModelDataBehavior(Action<IReadOnlyItemInstance, T> action)
        {
            _action = action;
        }

        public void Build(ItemBehaviorBuilder<object, object> item)
        {
            item.Add(ModelData.ItemAttribute, (context, _, next) =>
            {
                var modelData = next();
                modelData.CreateOrExtend<T>(d => _action(context, d));
                return modelData;
            });
        }
    }
    
    public sealed class CustomItemModelDataBehavior<TData, T> : IItemBehavior<TData>
        where TData : IData<TData>
        where T : notnull, new()
    {
        private readonly Action<IReadOnlyItemInstance, TData, T> _action;

        public CustomItemModelDataBehavior(Action<IReadOnlyItemInstance, TData, T> action)
        {
            _action = action;
        }

        public void Build(ItemBehaviorBuilder<TData, TData> item)
        {
            item.Add(ModelData.ItemAttribute, (context, data, next) =>
            {
                var modelData = next();
                modelData.CreateOrExtend<T>(d => _action(context, data, d));
                return modelData;
            });
        }

        public bool Equals(TData first, TData second) => true;
    }
}