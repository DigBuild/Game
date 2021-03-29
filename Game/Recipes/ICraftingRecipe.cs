using DigBuild.Engine.Items;
using DigBuild.Engine.Registries;
using DigBuild.Platform.Resource;

namespace DigBuild.Recipes
{
    public interface ICraftingRecipe
    {
        public ICraftingIngredient GetShapedInput(byte slot);
        public ICraftingIngredient GetShapelessInput(byte slot);
        public ICraftingIngredient GetCatalystInput();

        public CraftingOutput? GetOutput(ICraftingInput input);
    }

    public interface ICraftingInput
    {
        public ItemInstance GetCatalyst();
        public ItemInstance GetShaped(byte slot);
        public ItemInstance GetShapeless(byte slot);
    }

    public readonly struct CraftingOutput
    {
        public readonly ItemInstance Catalyst;
        public readonly ItemInstance Output;

        public CraftingOutput(ItemInstance catalyst, ItemInstance output)
        {
            Catalyst = catalyst;
            Output = output;
        }
    }

    public interface ICraftingIngredient
    {
        public bool Test(ItemInstance item);

        public ItemInstance ConsumeOne(ItemInstance item)
        {
            return new(item.Type, (ushort) (item.Count - 1u));
        }
    }

    public static class CraftingRecipeRegistryBuilderExtensions
    {
        public static T Add<T>(this IRegistryBuilder<ICraftingRecipe> registry, ResourceName name, T recipe)
            where T : ICraftingRecipe
        {
            return registry.Add(name, recipe);
        }
    }
}