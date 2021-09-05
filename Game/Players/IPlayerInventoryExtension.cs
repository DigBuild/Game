using DigBuild.Engine.Items.Inventories;
using DigBuild.Items;

namespace DigBuild.Players
{
    /// <summary>
    /// A player inventory extension.
    /// </summary>
    public interface IPlayerInventoryExtension
    {
        /// <summary>
        /// The inventory that extends the player's.
        /// </summary>
        IInventory Inventory { get; }
    }
}