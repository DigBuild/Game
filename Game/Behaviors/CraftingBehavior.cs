using System;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Items;
using DigBuild.Recipes;

namespace DigBuild.Behaviors
{
    public interface ICraftingBehavior
    {
        public InventorySlot[] ShapedSlots { get; }
        public InventorySlot[] ShapelessSlots { get; }
        public InventorySlot CatalystSlot { get; }

        public ICraftingRecipe? ActiveRecipe { set; }
        public CraftingOutput? ActiveRecipeOutput { set; }
    }

    public sealed class CraftingBehavior : IBlockBehavior<ICraftingBehavior>
    {
        public void Init(IBlockContext context, ICraftingBehavior data)
        {
            var craftingInput = new CraftingInput(data);
            void InputChanged() => OnInputChanged(context, data, craftingInput);
            foreach (var slot in data.ShapedSlots)
                slot.Changed += InputChanged;
            foreach (var slot in data.ShapelessSlots)
                slot.Changed += InputChanged;
            data.CatalystSlot.Changed += InputChanged;
        }

        public void Build(BlockBehaviorBuilder<ICraftingBehavior> block)
        {
        }

        private void OnInputChanged(IBlockContext context, ICraftingBehavior data, ICraftingInput craftingInput)
        {
            var lookupResult = Game.RecipeLookup.Find(craftingInput);
            data.ActiveRecipe = lookupResult?.Recipe;
            data.ActiveRecipeOutput = lookupResult?.Output;
        }

        private sealed class CraftingInput : ICraftingInput
        {
            private readonly ICraftingBehavior _data;

            public CraftingInput(ICraftingBehavior data)
            {
                _data = data;
            }

            public ItemInstance GetCatalyst() => _data.CatalystSlot.Item;

            public ItemInstance GetShaped(byte slot) => _data.ShapedSlots[slot].Item;

            public ItemInstance GetShapeless(byte slot) => _data.ShapelessSlots[slot].Item;
        }
    }
}