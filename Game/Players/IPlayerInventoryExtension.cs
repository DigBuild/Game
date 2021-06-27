using DigBuild.Items;

namespace DigBuild.Players
{
    public interface IPlayerInventoryExtension
    {
        IInventory Inventory { get; }
    }
}