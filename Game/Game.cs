using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using DigBuild.Blocks;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Items;
using DigBuild.Engine.Math;
using DigBuild.Engine.Voxel;
using DigBuild.Engine.Worldgen;
using DigBuild.Items;
using DigBuild.Voxel;
using DigBuild.Worldgen;

namespace DigBuild
{
    public class Game
    {
        public const string Domain = "digbuild";

        private readonly TickManager _tickManager;
        private readonly GameWindow _window;
        
        private readonly GameInput _input = new();
        private readonly HashSet<ChunkPos> _visitedChunks = new();

        private readonly World _world;
        private readonly PlayerController _player;
        private readonly WorldRayCastContext _rayCastContext;

        public Game()
        {
            GameRegistries.Initialize();
            
            var features = new List<IWorldgenFeature>
            {
                WorldgenFeatures.Terrain,
                WorldgenFeatures.Water
            };
            var generator = new WorldGenerator(features, 0, pos => new ChunkPrototype(pos));
            _world = new World(generator);
            _player = new PlayerController(_world, new Vector3(0, 15, 0));
            _player.Hotbar[0].Item = new ItemInstance(GameItems.Stone, 5);
            _player.Hotbar[1].Item = new ItemInstance(GameItems.Grass, 20);
            _player.Hotbar[3].Item = new ItemInstance(GameItems.Dirt, 12);
            _player.Hotbar[4].Item = new ItemInstance(GameItems.TriangleItem, 8);

            _rayCastContext = new WorldRayCastContext(_world);
            
            _tickManager = new TickManager(Tick);
            _window = new GameWindow(_tickManager, _player, _rayCastContext);

            _world.ChunkManager.ChunkChanged += chunk => _window.OnChunkChanged(chunk);
        }
        
        private void Tick()
        {
            _input.Update();
            
            _player.UpdateRotation(_input.PitchDelta, _input.YawDelta);
            _player.ApplyMotion(_input.ForwardDelta, _input.SidewaysDelta);
            if (_input.Jump)
                _player.Jump(_input.ForwardDelta);
            _player.Move();

            _player.CycleHotbar(
                (!_input.PrevCycleRight && _input.CycleRight ? 1 : 0) +
                (!_input.PrevCycleLeft && _input.CycleLeft ? -1 : 0)
            );
            if (!_input.PrevSwapUp && _input.SwapUp)
                _player.TransferHotbarUp();
            if (!_input.PrevSwapDown && _input.SwapDown)
                _player.TransferHotbarDown();

            var hit = RayCaster.Cast(_rayCastContext, _player.GetCamera(0).Ray);

            if (!_input.PrevActivate && _input.Activate)
            {
                var itemResult = _player.Hand.Item.Count > 0 ?
                    _player.Hand.Item.Item.OnActivate(new PlayerItemContext(_player.Hand.Item, _world), new ItemEvent.Activate(hit)) :
                    ItemEvent.Activate.Result.Fail;
                Console.WriteLine($"Interacted with item in slot {_player.ActiveHotbarSlot}! Result: {itemResult}");

                if (itemResult == ItemEvent.Activate.Result.Fail && hit != null)
                {
                    var block = _world.GetBlock(hit.BlockPos)!;
                    var blockResult = block.OnActivate(
                        new BlockContext(_world, hit.BlockPos, block),
                        new BlockEvent.Activate(hit)
                    );
                    Console.WriteLine($"Interacted with block at {hit.BlockPos} on face {hit.Face}! Result: {blockResult}"); 
                }
            }

            if (!_input.PrevPunch && _input.Punch)
            {
                var itemResult = _player.Hand.Item.Count > 0 ?
                    _player.Hand.Item.Item.OnPunch(new PlayerItemContext(_player.Hand.Item, _world), new ItemEvent.Punch(hit)) :
                    ItemEvent.Punch.Result.Fail;
                Console.WriteLine($"Punched with item {_player.ActiveHotbarSlot}! Result: {itemResult}");

                if (itemResult == ItemEvent.Punch.Result.Fail && hit != null)
                {
                    var block = _world.GetBlock(hit.BlockPos)!;
                    var blockResult = block.OnPunch(
                        new BlockContext(_world, hit.BlockPos, block),
                        new BlockEvent.Punch(hit)
                    );
                    Console.WriteLine($"Punched block at {hit.BlockPos} on face {hit.Face}! Result: {blockResult}"); 
                }
            }

            const int range = 5;
            const int rangeY = 3;

            var chunkPos = new BlockPos(_player.Position).ChunkPos;
            for (var x = -range; x < range; x++)
            {
                for (var z = -range; z < range; z++)
                {
                    for (var y = 0; y < rangeY; y++)
                    {
                        var p = new ChunkPos(chunkPos.X + x, y, chunkPos.Z + z);
                        if (_visitedChunks.Add(p)) // Only visit new chunks
                            _window.OnChunkChanged(_world.GetChunk(p)!);
                    }
                }
            }
        }

        public static async Task Main()
        {
            var game = new Game();
            await game._window.OpenWaitClosed();
        }
    }
}