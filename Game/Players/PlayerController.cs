using DigBuild.Blocks;
using DigBuild.Controller;
using DigBuild.Engine.Items;
using DigBuild.Engine.Worlds.Impl;
using DigBuild.Items;
using DigBuild.Worlds;

namespace DigBuild.Players
{
    /// <summary>
    /// A player controller.
    /// </summary>
    public sealed class PlayerController
    {
        private readonly IPlayer _player;

        /// <summary>
        /// Whether a hotbar transfer is currently taking place.
        /// </summary>
        public bool HotbarTransfer { get; set; }

        public PlayerController(IPlayer player)
        {
            _player = player;
        }
        
        /// <summary>
        /// Updates the player's movement.
        /// </summary>
        /// <param name="input">The game's input</param>
        public void UpdateMovement(GameInput input)
        {
            var physicalEntity = _player.PhysicsEntity;
            physicalEntity.Rotate(input.PitchDelta, input.YawDelta);
            physicalEntity.ApplyMotion(input.ForwardDelta, input.SidewaysDelta);
            if (input.Jump)
                physicalEntity.ApplyJumpMotion(input.ForwardDelta);
        }

        /// <summary>
        /// Updates the player's hotbar.
        /// </summary>
        /// <param name="input">The game's input</param>
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

        /// <summary>
        /// Updates the player's interactions.
        /// </summary>
        /// <param name="input">The game's input</param>
        /// <param name="hit">The player's raycast hit</param>
        /// <returns>Whether the operation was successful or not</returns>
        public bool UpdateInteraction(GameInput input, WorldRayCastContext.Hit? hit)
        {
            var world = _player.Entity.World;
            var hand = _player.Inventory.Hand;
            
            if (!input.PrevActivate && input.Activate)
            {
                var itemResult = hand.Item.Count > 0 ?
                    hand.Item.OnActivate(_player, hit) :
                    ItemEvent.Activate.Result.Fail;

                if (itemResult == ItemEvent.Activate.Result.Success)
                    return true;

                if (hit != null)
                {
                    var block = world.GetBlock(hit.BlockPos)!;
                    var blockResult = block.OnActivate(world, hit.BlockPos, hit, _player);
                    if (blockResult == BlockEvent.Activate.Result.Success)
                        return true;
                }
            }
            
            if (!input.PrevPunch && input.Punch)
            {
                var itemResult = hand.Item.Count > 0 ?
                    hand.Item.OnPunch(_player, hit) :
                    ItemEvent.Punch.Result.Fail;

                if (itemResult == ItemEvent.Punch.Result.Success)
                    return true;
            
                if (hit != null)
                {
                    var block = world.GetBlock(hit.BlockPos)!;
                    var blockResult = block.OnPunch(world, hit.BlockPos, hit, _player);
                    if (blockResult == BlockEvent.Punch.Result.Success)
                        return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Transfers an item out of the hotbar.
        /// </summary>
        public void TransferHotbarUp()
        {
            if (_player.Inventory.PickedItem.Item.Count > 0) return;
            _player.Inventory.PickedItem.TrySetItem(_player.Inventory.Hand.Item);
            _player.Inventory.Hand.TrySetItem(ItemInstance.Empty);
            HotbarTransfer = true;
        }

        /// <summary>
        /// Transfers an item into the hotbar.
        /// </summary>
        public void TransferHotbarDown()
        {
            if (_player.Inventory.PickedItem.Item.Count == 0) return;
            var hand = _player.Inventory.Hand.Item;
            _player.Inventory.Hand.TrySetItem(_player.Inventory.PickedItem.Item);
            _player.Inventory.PickedItem.TrySetItem(hand);
            HotbarTransfer = true;
        }
    }
}