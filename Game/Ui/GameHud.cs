using DigBuild.Controller;
using DigBuild.Engine.Impl.Worlds;
using DigBuild.Engine.Math;
using DigBuild.Engine.Physics;
using DigBuild.Engine.Render;
using DigBuild.Engine.Ui;
using DigBuild.Platform.Input;
using DigBuild.Platform.Render;
using DigBuild.Render;

namespace DigBuild.Ui
{
    public sealed class GameHud
    {
        private readonly GameplayController _controller;

        private UiContainer _ui = null!;
        private UiLabel _positionLabel = null!;
        private UiLabel _lookLabel = null!;
        private UiLabel _lightLabel = null!;
        private UiLabel _handLabel = null!;
        
        public GameHud(GameplayController controller)
        {
            _controller = controller;
        }

        public void Setup(IRenderTarget renderTarget)
        {
            _ui = new UiContainer();

            _ui.Add(20, 20, _positionLabel = new UiLabel(""));
            _ui.Add(20, 50, _lookLabel = new UiLabel(""));
            _ui.Add(20, 80, _lightLabel = new UiLabel(""));
            _ui.Add(20, 110, _handLabel = new UiLabel(""));

            var player = _controller.Player;
            var itemModels = _controller.Game.ModelManager.ItemModels;

            {
                var off = 60u;
                var i = 0;
                foreach (var slot in player.Inventory.Hotbar)
                {
                    var i1 = i;
                    _ui.Add(off, renderTarget.Height - 60, new UiInventorySlot(
                        slot, player.Inventory.PickedItem, itemModels, UiRenderLayer.Ui, 
                        () => player.Inventory.ActiveHotbarSlot == i1)
                    );
                    off += 100;
                    i++;
                }
            }

            _ui.Add(0, 0, new UiUnboundInventorySlot(player.Inventory.PickedItem, itemModels));
        }

        public void UpdateAndDraw(RenderContext context, GeometryBufferSet buffers, float partialTick)
        {
            var player = _controller.Player;
            var hit = Raycast.Cast(_controller.RayCastContext, player.GetCamera(partialTick).Ray);

            _positionLabel.Text = $"Position: {new BlockPos(player.PhysicalEntity.Position)}";
            _lookLabel.Text = $"Look: {hit?.Position} {(hit != null ? player.Entity.World.GetBlock(hit.BlockPos) : null)}";
            _lightLabel.Text = $"Light: {(hit == null ? "" : player.Entity.World.GetLight(hit.BlockPos.Offset(hit.Face)))}";
            _handLabel.Text = $"Hand: {player.Inventory.Hand.Item}";

            _ui.Draw(context, buffers, partialTick);
        }

        public void OnCursorMove(IUiElementContext context, int x, int y)
        {
            _ui.OnCursorMoved(context, x, y);
        }

        public void OnMouseEvent(IUiElementContext context, uint button, MouseAction action)
        {
            _ui.OnMouseEvent(context, button, action);
        }
    }
}