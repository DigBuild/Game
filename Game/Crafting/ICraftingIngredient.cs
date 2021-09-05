using DigBuild.Engine.Items;

namespace DigBuild.Crafting
{
    /// <summary>
    /// A crafting ingredient.
    /// </summary>
    public interface ICraftingIngredient
    {
        /// <summary>
        /// Checks if the specified item matches this ingredient.
        /// </summary>
        /// <param name="item">The item</param>
        /// <returns>Whether it matches or not</returns>
        bool Test(ItemInstance item);

        /// <summary>
        /// Outputs the result of consuming one of the given item.
        /// </summary>
        /// <param name="item">The item</param>
        /// <returns>The result</returns>
        ItemInstance ConsumeOne(ItemInstance item);
    }
}