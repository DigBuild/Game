using DigBuild.Engine.Items;

namespace DigBuild.Crafting
{
    /// <summary>
    /// The output of a crafting recipe.
    /// </summary>
    public readonly struct CraftingOutput
    {
        /// <summary>
        /// The catalyst.
        /// </summary>
        public readonly ItemInstance Catalyst;
        /// <summary>
        /// The crafting result.
        /// </summary>
        public readonly ItemInstance Output;

        public CraftingOutput(ItemInstance catalyst, ItemInstance output)
        {
            Catalyst = catalyst;
            Output = output;
        }
    }
}