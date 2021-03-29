using System;
using System.Collections.Generic;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.BuiltIn;
using DigBuild.Engine.Items;
using DigBuild.Recipes;

namespace DigBuild.Behaviors
{
    public interface IReadOnlyCraftingBehavior
    {
        public IReadOnlyList<IReadOnlyInventorySlot> ShapedSlots { get; }
        public IReadOnlyList<IReadOnlyInventorySlot> ShapelessSlots { get; }
        public IReadOnlyInventorySlot CatalystSlot { get; }
    }

    public interface ICraftingBehavior : IReadOnlyCraftingBehavior
    {
        public new IReadOnlyList<InventorySlot> ShapedSlots { get; }
        public new IReadOnlyList<InventorySlot> ShapelessSlots { get; }
        public new InventorySlot CatalystSlot { get; }

        public ICraftingRecipe? ActiveRecipe { set; }
        public CraftingOutput? ActiveRecipeOutput { set; }

        IReadOnlyList<IReadOnlyInventorySlot> IReadOnlyCraftingBehavior.ShapedSlots => ShapedSlots;
        IReadOnlyList<IReadOnlyInventorySlot> IReadOnlyCraftingBehavior.ShapelessSlots => ShapelessSlots;
        IReadOnlyInventorySlot IReadOnlyCraftingBehavior.CatalystSlot => CatalystSlot;
    }

    public sealed class CraftingBehavior : IBlockBehavior<IReadOnlyCraftingBehavior, ICraftingBehavior>
    {
        public void Init(ICraftingBehavior data)
        {
            var craftingInput = new CraftingInput(data);
            void InputChanged() => OnInputChanged(data, craftingInput);
            foreach (var slot in data.ShapedSlots)
                slot.Changed += InputChanged;
            foreach (var slot in data.ShapelessSlots)
                slot.Changed += InputChanged;
            data.CatalystSlot.Changed += InputChanged;
        }

        public void Build(BlockBehaviorBuilder<IReadOnlyCraftingBehavior, ICraftingBehavior> block)
        {
        }

        private void OnInputChanged(ICraftingBehavior data, ICraftingInput craftingInput)
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