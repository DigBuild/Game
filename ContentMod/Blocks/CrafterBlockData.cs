﻿using System.Collections.Generic;
using DigBuild.Content.Behaviors;
using DigBuild.Engine.Items;
using DigBuild.Engine.Storage;
using DigBuild.Recipes;

namespace DigBuild.Content.Blocks
{
    internal sealed class CrafterBlockData : IData<CrafterBlockData>, IFindCraftingRecipeBehavior, ICraftingUiBehavior
    {
        public IReadOnlyList<InventorySlot> ShapedSlots { get; } = new InventorySlot[]{new(), new(), new(), new(), new(), new(), new()};
        public IReadOnlyList<InventorySlot> ShapelessSlots { get; } = new InventorySlot[]{new(), new(), new(), new()};
        public InventorySlot CatalystSlot { get; } = new();
        public InventorySlot OutputSlot { get; } = new();

        public ICraftingRecipe? ActiveRecipe
        {
            set {
            }
        }

        public CraftingOutput? ActiveRecipeOutput
        {
            set => OutputSlot.TrySetItem(value?.Output ?? ItemInstance.Empty);
        }

        public CrafterBlockData Copy()
        {
            var copy = new CrafterBlockData();
            for (var i = 0; i < ShapedSlots.Count; i++)
                copy.ShapedSlots[i].TrySetItem(ShapedSlots[i].Item.Copy());
            return copy;
        }
    }
}