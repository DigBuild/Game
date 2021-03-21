using DigBuild.Engine.Items;

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
}