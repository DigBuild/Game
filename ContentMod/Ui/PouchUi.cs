using DigBuild.Content.Behaviors;
using DigBuild.Engine.Items;
using DigBuild.Engine.Ui;
using DigBuild.Render;
using DigBuild.Ui;

namespace DigBuild.Content.Ui
{
    public static class PouchUi
    {
        public static IUi Create(IPouch pouch, IInventorySlot pickedItemSlot)
        {
            var itemModels = DigBuildGame.Instance.ModelManager.ItemModels;
            var container = new UiContainer();
            return new SimpleUi(container)
            {
                Resized = target =>
                {
                    container.Clear();

                    uint x = 120u, y = 120u;
                    var slots = new UiInventorySlot[pouch.Slots.Length];
                    for (var i = 0; i < slots.Length; i++)
                    {
                        container.Add(x, y, new UiInventorySlot(
                            pouch.Slots[i], pickedItemSlot, itemModels, UiRenderLayer.Ui, GameHud.InventorySlotSprite
                        ));
                        x += 60;
                        if (i % 5 == 4)
                        {
                            x -= 4 * 60 + 30;
                            y += 52;
                        }

                        if (i % 10 == 9)
                        {
                            x -= 60;
                        }
                    }
                }
            };
        }
    }
}