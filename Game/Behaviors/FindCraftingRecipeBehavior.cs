using System.Collections.Generic;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Items;
using DigBuild.Recipes;

namespace DigBuild.Behaviors
{
    public interface IFindCraftingRecipeBehavior
    {
        public IReadOnlyList<InventorySlot> ShapedSlots { get; }
        public IReadOnlyList<InventorySlot> ShapelessSlots { get; }
        public InventorySlot CatalystSlot { get; }

        public ICraftingRecipe? ActiveRecipe { set; }
        public CraftingOutput? ActiveRecipeOutput { set; }
    }

    public sealed class FindCraftingRecipeBehavior :
        IBlockBehavior<IFindCraftingRecipeBehavior, IFindCraftingRecipeBehavior>,
        IItemBehavior<IFindCraftingRecipeBehavior, IFindCraftingRecipeBehavior>
    {
        public void Init(IFindCraftingRecipeBehavior data)
        {
            var craftingInput = new CraftingInput(data);
            void InputChanged() => OnInputChanged(data, craftingInput);
            foreach (var slot in data.ShapedSlots)
                slot.Changed += InputChanged;
            foreach (var slot in data.ShapelessSlots)
                slot.Changed += InputChanged;
            data.CatalystSlot.Changed += InputChanged;
        }

        public void Build(BlockBehaviorBuilder<IFindCraftingRecipeBehavior, IFindCraftingRecipeBehavior> block)
        {
        }

        public void Build(ItemBehaviorBuilder<IFindCraftingRecipeBehavior, IFindCraftingRecipeBehavior> item)
        {
        }

        private void OnInputChanged(IFindCraftingRecipeBehavior data, ICraftingInput craftingInput)
        {
            var lookupResult = Game.RecipeLookup.Find(craftingInput);
            data.ActiveRecipe = lookupResult?.Recipe;
            data.ActiveRecipeOutput = lookupResult?.Output;
        }

        private sealed class CraftingInput : ICraftingInput
        {
            private readonly IFindCraftingRecipeBehavior _data;

            public CraftingInput(IFindCraftingRecipeBehavior data)
            {
                _data = data;
            }

            public ItemInstance GetCatalyst() => _data.CatalystSlot.Item;

            public ItemInstance GetShaped(byte slot) => _data.ShapedSlots[slot].Item;

            public ItemInstance GetShapeless(byte slot) => _data.ShapelessSlots[slot].Item;
        }
    }
}