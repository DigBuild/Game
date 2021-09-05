using System.Collections.Generic;
using DigBuild.Registries;

namespace DigBuild.Crafting
{
    /// <summary>
    /// A recipe lookup helper.
    /// </summary>
    public static class CraftingRecipeLookup
    {
        /// <summary>
        /// Tries to find a recipe matching the given input.
        /// </summary>
        /// <param name="input">The crafting input</param>
        /// <returns>The recipe and its output if found, otherwise null</returns>
        public static (ICraftingRecipe Recipe, CraftingOutput Output)? Find(ICraftingInput input)
        {
            foreach (var recipe in GameRegistries.CraftingRecipes.Values)
            {
                if (!recipe.GetCatalystInput().Test(input.GetCatalyst()))
                    continue;

                var matches = true;
                for (byte i = 0; i < 7; i++)
                {
                    if (recipe.GetShapedInput(i).Test(input.GetShaped(i)))
                        continue;

                    matches = false;
                    break;
                }
                if(!matches)
                    continue;
                
                for (byte i = 0; i < 4; i++)
                {
                    if (recipe.GetShapelessInput(i).Test(input.GetShapeless(i)))
                        continue;

                    matches = false;
                    break;
                }
                if(!matches)
                    continue;

                var output = recipe.GetOutput(input);
                if (output.HasValue)
                    return (recipe, output.Value);
            }

            return null;
        }
    }
}