using System;
using System.Numerics;
using DigBuild.Blocks;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Items;
using DigBuild.Engine.Worlds;
using DigBuild.Registries;

namespace DigBuild.Behaviors
{
    public sealed class DropItemBehavior : IBlockBehavior
    {
        private readonly Func<ItemInstance> _dropSupplier;

        public DropItemBehavior(Func<ItemInstance> dropSupplier)
        {
            _dropSupplier = dropSupplier;
        }

        public void Build(BlockBehaviorBuilder<object, object> block)
        {
            block.Subscribe(OnBreaking);
        }

        private void OnBreaking(IBlockContext context, object data, BlockEvent.Breaking evt, Action next)
        {
            context.World.AddEntity(GameEntities.Item)
                .WithPosition(((Vector3) context.Pos) + new Vector3(0.5f, 0, 0.5f))
                .WithItem(_dropSupplier());
            next();
        }
    }
}