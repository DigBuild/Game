using System.Collections.Generic;
using DigBuild.Engine.Items;
using DigBuild.Engine.Render;
using DigBuild.Engine.Ui;
using DigBuild.Render;
using DigBuild.Ui;

namespace DigBuild.Content.Ui
{
    public static class CraftingUi
    {
        public static IUiElement Create(ICraftingInventory inventory, IInventorySlot pickedItemSlot, IReadOnlyDictionary<Item, IItemModel> itemModels)
        {
            var ui = new UiContainer();

            uint x = 120u, y = 120u;
            var shapedSlots = new UiInventorySlot[inventory.ShapedSlots.Count];
            for (var i = 0; i < shapedSlots.Length; i++)
            { 
                ui.Add(x, y, new UiInventorySlot(
                    inventory.ShapedSlots[i], pickedItemSlot, itemModels, UiRenderLayer.Ui
                ));
                if (i == 1 || i == 4)
                {
                    x -= 45 * 3;
                    y += 76;
                }
                else
                {
                    x += 90;
                }
            }
            ui.Add(120 + 90 * 3, 120 + 76, new UiInventorySlot(
                inventory.OutputSlot, pickedItemSlot, itemModels, UiRenderLayer.Ui
            ));

            return ui;
        }
    }
}