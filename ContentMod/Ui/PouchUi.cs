using System.Numerics;
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

                    // 450 x 300
                    // 330 x 170

                    uint x = 220u, y = 120u;

                    x = (target.Width - 330) / 2 + 30;
                    y = (target.Height - 170) / 2 + 32;

                    container.Add(x - 60 - 30, y - 65 - 32, new UiRectangle(512, 512, UiRenderLayer.Ui, GameHud.PouchBackgroundSprite, Vector4.One));

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