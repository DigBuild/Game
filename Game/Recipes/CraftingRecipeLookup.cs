using System.Collections.Generic;

namespace DigBuild.Recipes
{
    public sealed class CraftingRecipeLookup
    {
        private readonly IEnumerable<ICraftingRecipe> _recipes;

        public CraftingRecipeLookup(IEnumerable<ICraftingRecipe> recipes)
        {
            _recipes = recipes;
        }

        public (ICraftingRecipe Recipe, CraftingOutput Output)? Find(ICraftingInput input)
        {
            foreach (var recipe in _recipes)
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