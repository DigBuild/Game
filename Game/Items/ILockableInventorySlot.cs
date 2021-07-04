using DigBuild.Engine.Items;

namespace DigBuild.Items
{
    public interface IReadOnlyLockableInventorySlot : IReadOnlyInventorySlot
    {
        bool IsLocked { get; }
    }

    public interface ILockableInventorySlot : IInventorySlot, IReadOnlyLockableInventorySlot
    {
        void ToggleLocked();
    }
}