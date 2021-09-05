using DigBuild.Crafting;
using DigBuild.Engine.Items;
using DigBuild.Engine.Registries;
using DigBuild.Platform.Resource;

namespace DigBuild.Content.Registries
{
    public static class GameRecipes
    {
        internal static void Register(RegistryBuilder<ICraftingRecipe> registry)
        {
            var stoneIngredient = new CraftingIngredient(GameItems.Stone);
            registry.Register(new ResourceName(DigBuildGame.Domain, "stone_to_stairs"),
                new CraftingRecipe(
                    new[]
                    {
                        CraftingIngredient.None, CraftingIngredient.None,
                        CraftingIngredient.None, stoneIngredient, CraftingIngredient.None,
                        stoneIngredient, stoneIngredient
                    },
                    new[]
                    {
                        CraftingIngredient.None, CraftingIngredient.None,
                        CraftingIngredient.None, CraftingIngredient.None
                    },
                    CraftingIngredient.None,
                    new ItemInstance(GameItems.StoneStairs, 3)
                )
            );
        }
    }
}