using DigBuild.Blocks;
using DigBuild.Client;
using DigBuild.Engine.Impl.Worlds;
using DigBuild.Engine.Items;
using DigBuild.Items;
using DigBuild.Worlds;

namespace DigBuild.Players
{
    
    public sealed class PlayerController
    {
        private readonly IPlayer _player;
        public bool HotbarTransfer { get; set; }

        public PlayerController(IPlayer player)
        {
            _player = player;
        }
        
        public void UpdateMovement(GameInput input)
        {
            var physicalEntity = _player.PhysicalEntity;
            physicalEntity.Rotate(input.PitchDelta, input.YawDelta);
            physicalEntity.ApplyMotion(input.ForwardDelta, input.SidewaysDelta);
            if (input.Jump)
                physicalEntity.ApplyJumpMotion(input.ForwardDelta);
        }

        public void UpdateHotbar(GameInput input)
        {
            _player.Inventory.CycleHotbar(
                (!input.PrevCycleRight && input.CycleRight ? 1 : 0) +
                (!input.PrevCycleLeft && input.CycleLeft ? -1 : 0)
            );
            if (!input.PrevSwapUp && input.SwapUp)
                TransferHotbarUp();
            if (!input.PrevSwapDown && input.SwapDown)
                TransferHotbarDown();
        }

        public void UpdateInteraction(GameInput input, WorldRayCastContext.Hit? hit)
        {
            var world = _player.Entity.World;
            ref var hand = ref _player.Inventory.Hand;
            
            if (!input.PrevActivate && input.Activate)
            {
                var itemResult = hand.Item.Count > 0 ?
                    hand.Item.Type.OnActivate(hand.Item, _player, hit) :
                    ItemEvent.Activate.Result.Fail;
            
                if (itemResult == ItemEvent.Activate.Result.Fail && hit != null)
                {
                    var block = world.GetBlock(hit.BlockPos)!;
                    var blockResult = block.OnActivate(world, hit.BlockPos, hit);
                }
            }
            
            if (!input.PrevPunch && input.Punch)
            {
                var itemResult = hand.Item.Count > 0 ?
                    hand.Item.Type.OnPunch(hand.Item, _player, hit) :
                    ItemEvent.Punch.Result.Fail;
            
                if (itemResult == ItemEvent.Punch.Result.Fail && hit != null)
                {
                    var block = world.GetBlock(hit.BlockPos)!;
                    var blockResult = block.OnPunch(world, hit.BlockPos, hit);
                }
            }
        }

        public void TransferHotbarUp()
        {
            if (_player.Inventory.PickedItem.Item.Count > 0) return;
            _player.Inventory.PickedItem.Item = _player.Inventory.Hand.Item;
            _player.Inventory.Hand.Item = ItemInstance.Empty;
            HotbarTransfer = true;
        }

        public void TransferHotbarDown()
        {
            if (_player.Inventory.PickedItem.Item.Count == 0) return;
            var hand = _player.Inventory.Hand.Item;
            _player.Inventory.Hand.Item = _player.Inventory.PickedItem.Item;
            _player.Inventory.PickedItem.Item = hand;
            HotbarTransfer = true;
        }
    }
}