using DigBuild.Controller;
using DigBuild.Engine.Impl.Worlds;
using DigBuild.Engine.Math;
using DigBuild.Engine.Physics;
using DigBuild.Engine.Render;
using DigBuild.Engine.Textures;
using DigBuild.Engine.Ui;
using DigBuild.Events;
using DigBuild.Platform.Input;
using DigBuild.Platform.Render;
using DigBuild.Render;

namespace DigBuild.Ui
{
    public sealed class GameHud : IUi
    {
        private static ISprite EquipmentButtonSprite { get; set; } = null!;
        private static ISprite EquipmentButtonSprite2 { get; set; } = null!;
        private static ISprite EquipmentButtonSprite3 { get; set; } = null!;

        public static void OnUiTextureStitching(UiTextureStitchingEvent evt)
        {
            var stitcher = evt.Stitcher;
            var resourceManager = evt.ResourceManager;
            
            EquipmentButtonSprite = stitcher.Add(resourceManager.Get<BitmapTexture>(DigBuildGame.Domain, "textures/ui/button_inactive.png")!);
            EquipmentButtonSprite2 = stitcher.Add(resourceManager.Get<BitmapTexture>(DigBuildGame.Domain, "textures/ui/button_hovered.png")!);
            EquipmentButtonSprite3 = stitcher.Add(resourceManager.Get<BitmapTexture>(DigBuildGame.Domain, "textures/ui/button_clicked.png")!);
        }

        private readonly GameplayController _controller;

        private readonly UiContainer _ui = new();
        private SimpleUi.Context _context = null!;

        private UiLabel _positionLabel = null!;
        private UiLabel _lookLabel = null!;
        private UiLabel _lightLabel = null!;
        private UiLabel _handLabel = null!;

        private bool _isTop;
        private bool _isMouseFree;

        public CursorMode CursorMode => _isMouseFree ? CursorMode.Normal : CursorMode.Raw;

        public GameHud(GameplayController controller)
        {
            _controller = controller;
        }

        public void OnOpened(UiManager manager)
        {
            _context = new SimpleUi.Context(this, manager);
            _isTop = true;
        }

        public void OnResized(IRenderTarget target)
        {
            _ui.Clear();

            _ui.Add(20, 20, _positionLabel = new UiLabel(""));
            _ui.Add(20, 50, _lookLabel = new UiLabel(""));
            _ui.Add(20, 80, _lightLabel = new UiLabel(""));
            _ui.Add(20, 110, _handLabel = new UiLabel(""));

            const int slotSize = (int)UiInventorySlot.Scale;

            var player = _controller.Player;
            var itemModels = _controller.Game.ModelManager.ItemModels;

            {
                var width = (player.Inventory.Hotbar.Count + 1) * slotSize * 2;

                var off = (int) (target.Width - width) / 2 + slotSize;
                var i = 0;
                foreach (var slot in player.Inventory.Hotbar)
                {
                    var i1 = i;
                    _ui.Add(off, (int) target.Height - 60, new UiInventorySlot(
                        slot, player.Inventory.PickedItem, itemModels, UiRenderLayer.Ui, 
                        () => player.Inventory.ActiveHotbarSlot == i1)
                    );
                    off += slotSize * 2;
                    if (i == player.Inventory.Hotbar.Count / 2 - 1)
                    {
                        off += slotSize * 2;
                    }
                    i++;
                }
            }

            var equipmentButton = new UiButton(slotSize * 2, slotSize * 2, UiRenderLayer.Ui,
                EquipmentButtonSprite, EquipmentButtonSprite2, EquipmentButtonSprite3);
            _ui.Add(target.Width / 2 - slotSize, target.Height - 60 - slotSize, equipmentButton);

            var equipmentContainer = new UiContainer {Visible = false};
            {
                const int distance = slotSize * 2 + 10;
                equipmentContainer.Add(0, -50 - 0 * distance, new UiInventorySlot(
                    player.Inventory.Equipment.Boots, player.Inventory.PickedItem, itemModels, UiRenderLayer.Ui, () => false)
                );
                equipmentContainer.Add(0, -50 - 1 * distance, new UiInventorySlot(
                    player.Inventory.Equipment.Leggings, player.Inventory.PickedItem, itemModels, UiRenderLayer.Ui, () => false)
                );
                equipmentContainer.Add(0, -50 - 2 * distance, new UiInventorySlot(
                    player.Inventory.Equipment.Chestplate, player.Inventory.PickedItem, itemModels, UiRenderLayer.Ui, () => false)
                );
                equipmentContainer.Add(0, -50 - 3 * distance, new UiInventorySlot(
                    player.Inventory.Equipment.Helmet, player.Inventory.PickedItem, itemModels, UiRenderLayer.Ui, () => false)
                );
                
                equipmentContainer.Add(-slotSize * 2, -50 - (int) (2.5 * distance), new UiInventorySlot(
                    player.Inventory.Equipment.EquipTopLeft, player.Inventory.PickedItem, itemModels, UiRenderLayer.Ui, () => false)
                );
                equipmentContainer.Add(slotSize * 2, -50 - (int) (2.5 * distance), new UiInventorySlot(
                    player.Inventory.Equipment.EquipTopRight, player.Inventory.PickedItem, itemModels, UiRenderLayer.Ui, () => false)
                );
                equipmentContainer.Add(-slotSize * 2, -50 - (int) (1.5 * distance), new UiInventorySlot(
                    player.Inventory.Equipment.EquipBottomLeft, player.Inventory.PickedItem, itemModels, UiRenderLayer.Ui, () => false)
                );
                equipmentContainer.Add(slotSize * 2, -50 - (int) (1.5 * distance), new UiInventorySlot(
                    player.Inventory.Equipment.EquipBottomRight, player.Inventory.PickedItem, itemModels, UiRenderLayer.Ui, () => false)
                );
            }
            _ui.Add(target.Width / 2, target.Height - 60 - slotSize, equipmentContainer);

            // Toggle equipment container with button
            equipmentButton.Released += () => equipmentContainer.Visible = !equipmentContainer.Visible;

            _ui.Add(0, 0, new UiUnboundInventorySlot(player.Inventory.PickedItem, itemModels));
        }

        public void OnClosed()
        {
        }

        public void UpdateAndDraw(RenderContext context, IGeometryBuffer buffer, float partialTick)
        {
            var player = _controller.Player;
            var hit = Raycast.Cast(_controller.RayCastContext, player.GetCamera(partialTick).Ray);

            _positionLabel.Text = $"Position: {new BlockPos(player.PhysicalEntity.Position)}";
            _lookLabel.Text = $"Look: {hit?.Position} {(hit != null ? player.Entity.World.GetBlock(hit.BlockPos) : null)}";
            _lightLabel.Text = $"Light: {(hit == null ? "" : player.Entity.World.GetLight(hit.BlockPos.Offset(hit.Face)))}";
            _handLabel.Text = $"Hand: {player.Inventory.Hand.Item}";

            _ui.Draw(context, buffer, partialTick);
        }

        public bool OnCursorMoved(int x, int y)
        {
            if (_isMouseFree)
                _ui.OnCursorMoved(_context, x, y);
            else
                _controller.InputController.OnCursorMoved((uint) x, (uint) y);

            return true;
        }

        public bool OnMouseEvent(uint button, MouseAction action)
        {
            if (_isMouseFree)
                _ui.OnMouseEvent(_context, button, action);
            else
                _controller.InputController.OnMouseEvent(button, action);

            return true;
        }

        public bool OnKeyboardEvent(uint code, KeyboardAction action)
        {
            if (code == 56)
            {
                _isMouseFree = _isTop && action != KeyboardAction.Release;
                return true;
            }

            if (_isMouseFree)
                _context.KeyboardEventDelegate?.Invoke(code, action);
            else
                _controller.InputController.OnKeyboardEvent(code, action);

            return true;
        }

        public void OnLayerAdded()
        {
            _isTop = false;
        }

        public void OnLayerRemoved()
        {
            _isTop = true;
        }
    }
}