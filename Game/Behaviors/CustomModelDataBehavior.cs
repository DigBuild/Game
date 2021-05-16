using System;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Render;
using DigBuild.Engine.Render.Models;
using DigBuild.Engine.Storage;

namespace DigBuild.Behaviors
{
    public sealed class CustomModelDataBehavior<T> : IBlockBehavior where T : notnull, new()
    {
        private readonly Action<IReadOnlyBlockContext, T> _action;

        public CustomModelDataBehavior(Action<IReadOnlyBlockContext, T> action)
        {
            _action = action;
        }

        public void Build(BlockBehaviorBuilder<object, object> block)
        {
            block.Add(ModelData.BlockAttribute, (context, _, next) =>
            {
                var modelData = next();
                modelData.CreateOrExtend<T>(d => _action(context, d));
                return modelData;
            });
        }
    }
    
    public sealed class CustomModelDataBehavior<TData, T> : IBlockBehavior<TData>
        where TData : IData<TData>
        where T : notnull, new()
    {
        private readonly Action<IReadOnlyBlockContext, TData, T> _action;

        public CustomModelDataBehavior(Action<IReadOnlyBlockContext, TData, T> action)
        {
            _action = action;
        }

        public void Build(BlockBehaviorBuilder<TData, TData> block)
        {
            block.Add(ModelData.BlockAttribute, (context, data, next) =>
            {
                var modelData = next();
                modelData.CreateOrExtend<T>(d => _action(context, data, d));
                return modelData;
            });
        }
    }
}