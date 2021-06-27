using DigBuild.Engine.Items;
using DigBuild.Engine.Ui;
using DigBuild.Render;
using DigBuild.Ui;

namespace DigBuild.Content.Ui
{
    public static class CraftingUi
    {
        public static IUi Create(ICraftingInventory inventory, IInventorySlot pickedItemSlot)
        {
            var itemModels = DigBuildGame.Instance.ModelManager.ItemModels;
            var container = new UiContainer();
            return new SimpleUi(container)
            {
                Resized = target =>
                {
                    container.Clear();

                    uint x = 120u, y = 120u;
                    var shapedSlots = new UiInventorySlot[inventory.ShapedSlots.Count];
                    for (var i = 0; i < shapedSlots.Length; i++)
                    {
                        container.Add(x, y, new UiInventorySlot(
                            inventory.ShapedSlots[i], pickedItemSlot, itemModels, UiRenderLayer.Ui
                        ));
                        if (i is 1 or 4)
                        {
                            x -= 45 * 3;
                            y += 76;
                        }
                        else
                        {
                            x += 90;
                        }
                    }

                    container.Add(120 + 90 * 3, 120 + 76, new UiInventorySlot(
                        inventory.OutputSlot, pickedItemSlot, itemModels, UiRenderLayer.Ui
                    ));
                }
            };
        }
    }
}