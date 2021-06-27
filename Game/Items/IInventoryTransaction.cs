using System;
using DigBuild.Engine.Items;

namespace DigBuild.Items
{
    public interface IInventoryTransaction
    {
        ItemInstance Insert(ItemInstance item);
        ItemInstance Extract(ushort amount, Func<IReadOnlyItemInstance, bool> test = null!);

        void Checkpoint();
        void Rollback();

        void Commit();
    }
}