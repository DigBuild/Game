using System;
using DigBuild.Engine.Items;

namespace DigBuild.Crafting
{
    public sealed class CraftingRecipe : ICraftingRecipe
    {
        private readonly ICraftingIngredient[] _shaped;
        private readonly ICraftingIngredient[] _shapeless;
        private readonly ICraftingIngredient _catalyst;
        private readonly ItemInstance _output;
        private readonly Func<ItemInstance, ItemInstance> _catalystOutput;

        public CraftingRecipe(
            ICraftingIngredient[] shaped,
            ICraftingIngredient[] shapeless,
            ICraftingIngredient catalyst,
            ItemInstance output,
            Func<ItemInstance, ItemInstance> catalystOutput
        )
        {
            _shaped = shaped;
            _shapeless = shapeless;
            _catalyst = catalyst;
            _output = output;
            _catalystOutput = catalystOutput;
        }

        public CraftingRecipe(
            ICraftingIngredient[] shaped,
            ICraftingIngredient[] shapeless,
            ICraftingIngredient catalyst,
            ItemInstance output
        ) : this(shaped, shapeless, catalyst, output, i => i)
        {
        }

        public ICraftingIngredient GetShapedInput(byte slot) => _shaped[slot];
        public ICraftingIngredient GetShapelessInput(byte slot) => _shapeless[slot];
        public ICraftingIngredient GetCatalystInput() => _catalyst;

        public CraftingOutput? GetOutput(ICraftingInput input)
        {
            return new CraftingOutput(_catalystOutput(input.GetCatalyst()), _output);
        }
    }

    public sealed class CraftingIngredient : ICraftingIngredient
    {
        public static readonly ICraftingIngredient None = new NoIngredient();

        private readonly Item _item;

        public CraftingIngredient(Item item)
        {
            _item = item;
        }

        public bool Test(ItemInstance item)
        {
            return item.Count > 0 && item.Type == _item;
        }

        private sealed class NoIngredient : ICraftingIngredient
        {
            public bool Test(ItemInstance item)
            {
                return item.Count == 0;
            }
        }
    }
}