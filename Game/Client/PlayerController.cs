using System;
using DigBuild.Behaviors;
using DigBuild.Blocks;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Entities;
using DigBuild.Engine.Items;
using DigBuild.Engine.Worlds;
using DigBuild.Items;
using DigBuild.Players;
using DigBuild.Worlds;

namespace DigBuild.Client
{
    public sealed class PlayerController : IPlayer
    {
        private readonly IPlayer _player;

        public EntityInstance Entity => _player.Entity;
        public IPhysicalEntity PhysicalEntity => _player.PhysicalEntity;
        public PlayerInventory Inventory => _player.Inventory;

        public bool HotbarTransfer { get; set; }

        public PlayerController(IPlayer player)
        {
            _player = player;
        }

        public IPlayerCamera GetCamera(float partialTick) => _player.GetCamera(partialTick);

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
                    hand.Item.Type.OnActivate(new ItemContext(hand.Item), new ItemEvent.Activate(_player, hit)) :
                    ItemEvent.Activate.Result.Fail;
                Console.WriteLine($"Interacted with item in slot {_player.Inventory.ActiveHotbarSlot}! Result: {itemResult}");

                if (itemResult == ItemEvent.Activate.Result.Fail && hit != null)
                {
                    var block = world.GetBlock(hit.BlockPos)!;
                    var blockResult = block.OnActivate(
                        new BlockContext(world, hit.BlockPos, block),
                        new BlockEvent.Activate(hit)
                    );
                    Console.WriteLine($"Interacted with block at {hit.BlockPos} on face {hit.Face}! Result: {blockResult}"); 
                }
            }

            if (!input.PrevPunch && input.Punch)
            {
                var itemResult = hand.Item.Count > 0 ?
                    hand.Item.Type.OnPunch(new ItemContext(hand.Item), new ItemEvent.Punch(_player, hit)) :
                    ItemEvent.Punch.Result.Fail;
                Console.WriteLine($"Punched with item {_player.Inventory.ActiveHotbarSlot}! Result: {itemResult}");

                if (itemResult == ItemEvent.Punch.Result.Fail && hit != null)
                {
                    var block = world.GetBlock(hit.BlockPos)!;
                    var blockResult = block.OnPunch(
                        new BlockContext(world, hit.BlockPos, block),
                        new BlockEvent.Punch(hit)
                    );
                    Console.WriteLine($"Punched block at {hit.BlockPos} on face {hit.Face}! Result: {blockResult}"); 
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