using System;
using System.Collections.Generic;
using DigBuild.Content.Behaviors;
using DigBuild.Engine.Items;
using DigBuild.Engine.Items.Inventories;
using DigBuild.Engine.Serialization;
using DigBuild.Engine.Storage;
using DigBuild.Recipes;

namespace DigBuild.Content.Blocks
{
    internal sealed class CrafterBlockData : IData<CrafterBlockData>, IChangeNotifier, IFindCraftingRecipeBehavior, ICraftingUiBehavior
    {
        private List<InventorySlot> _shapedSlots = new()
        {
            new InventorySlot(),
            new InventorySlot(),
            new InventorySlot(),
            new InventorySlot(),
            new InventorySlot(),
            new InventorySlot(),
            new InventorySlot(),
        };
        private List<InventorySlot> _shapelessSlots = new()
        {
            new InventorySlot(),
            new InventorySlot(),
            new InventorySlot(),
            new InventorySlot(),
        };
        private InventorySlot _outputSlot = new();

        public event Action? Changed;

        public IReadOnlyList<InventorySlot> ShapedSlots => _shapedSlots;
        public IReadOnlyList<InventorySlot> ShapelessSlots => _shapelessSlots;
        public InventorySlot CatalystSlot { get; } = new();
        public InventorySlot OutputSlot => _outputSlot;

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

        public static ISerdes<CrafterBlockData> Serdes { get; } = new CompositeSerdes<CrafterBlockData>()
        {
            { 1u, data => data._shapedSlots, SimpleSerdes.OfList(InventorySlot.Serdes) },
            { 2u, data => data._shapelessSlots, SimpleSerdes.OfList(InventorySlot.Serdes) },
            { 3u, data => data._outputSlot, InventorySlot.Serdes },
        };
    }
}