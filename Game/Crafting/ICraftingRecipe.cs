using DigBuild.Engine.Registries;
using DigBuild.Platform.Resource;

namespace DigBuild.Crafting
{
    /// <summary>
    /// A crafting recipe.
    /// </summary>
    public interface ICraftingRecipe
    {
        /// <summary>
        /// Gets the ingredient in the given shaped slot.
        /// </summary>
        /// <param name="slot">The slot ID</param>
        /// <returns>The ingredient</returns>
        public ICraftingIngredient GetShapedInput(byte slot);
        /// <summary>
        /// Gets the ingredient in the given shapeless slot.
        /// </summary>
        /// <param name="slot">The slot ID</param>
        /// <returns>The ingredient</returns>
        public ICraftingIngredient GetShapelessInput(byte slot);
        /// <summary>
        /// Gets the catalyst ingredient.
        /// </summary>
        /// <returns>The ingredient</returns>
        public ICraftingIngredient GetCatalystInput();

        /// <summary>
        /// Computes the recipe's output.
        /// </summary>
        /// <param name="input">The input</param>
        /// <returns>The output if matching, otherwise null</returns>
        public CraftingOutput? GetOutput(ICraftingInput input);
    }

    /// <summary>
    /// Registry extensions for crafting recipes.
    /// </summary>
    public static class CraftingRecipeRegistryBuilderExtensions
    {
        /// <summary>
        /// Registers a new crafting recipe.
        /// </summary>
        /// <typeparam name="T">The recipe type</typeparam>
        /// <param name="registry">The registry</param>
        /// <param name="name">The name</param>
        /// <param name="recipe">The recipe</param>
        /// <returns>The recipe</returns>
        public static T Register<T>(this IRegistryBuilder<ICraftingRecipe> registry, ResourceName name, T recipe)
            where T : ICraftingRecipe
        {
            return registry.Add(name, recipe);
        }
    }
}