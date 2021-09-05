using DigBuild.Engine.Items;

namespace DigBuild.Crafting
{
    /// <summary>
    /// The input to a crafting recipe.
    /// </summary>
    public interface ICraftingInput
    {
        /// <summary>
        /// Gets the catalyst item.
        /// </summary>
        /// <returns>The catalyst item</returns>
        ItemInstance GetCatalyst();
        /// <summary>
        /// Gets the shaped crafting item in the given slot.
        /// </summary>
        /// <param name="slot">The slot</param>
        /// <returns>The item</returns>
        ItemInstance GetShaped(byte slot);
        /// <summary>
        /// Gets the shapeless crafting item in the given slot.
        /// </summary>
        /// <param name="slot">The slot</param>
        /// <returns>The item</returns>
        ItemInstance GetShapeless(byte slot);
    }
}