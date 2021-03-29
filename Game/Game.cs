using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using DigBuild.Blocks;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Items;
using DigBuild.Engine.Math;
using DigBuild.Engine.Physics;
using DigBuild.Engine.Worldgen;
using DigBuild.Engine.Worlds;
using DigBuild.Items;
using DigBuild.Recipes;
using DigBuild.Voxel;
using DigBuild.Worldgen;

namespace DigBuild
{
    public class Game
    {
        public const string Domain = "digbuild";
        public const int ViewRadius = 16;
        public static CraftingRecipeLookup RecipeLookup { get; private set; } = null!;

        private readonly TickSource _tickSource;
        private readonly GameWindow _window;
        
        private readonly GameInput _input = new();

        private readonly World _world;
        private readonly PlayerController _player;
        private readonly WorldRayCastContext _rayCastContext;

        public Game()
        {
            GameRegistries.Initialize();

            _tickSource = new TickSource();
            _tickSource.Tick += Tick;

            var stoneIngredient = new CraftingIngredient(GameItems.Stone);
            var recipes = new List<ICraftingRecipe>
            {
                new CraftingRecipe(
                    new[]
                    {
                        CraftingIngredient.None, CraftingIngredient.None,
                        CraftingIngredient.None, stoneIngredient, CraftingIngredient.None,
                        stoneIngredient, stoneIngredient
                    },
                    new[]
                    {
                        CraftingIngredient.None, CraftingIngredient.None,
                        CraftingIngredient.None, CraftingIngredient.None
                    },
                    CraftingIngredient.None,
                    new ItemInstance(GameItems.Crafter, 3)
                )
            };
            RecipeLookup = new CraftingRecipeLookup(recipes);

            var features = new List<IWorldgenFeature>
            {
                WorldgenFeatures.Terrain,
                WorldgenFeatures.Water
            };
            var generator = new WorldGenerator(
                features, 0,
                desc => desc.Get(WorldgenAttributes.TerrainHeight).Max()
            );
            _world = new World(generator, _tickSource, () => _player == null ? default : new BlockPos(_player.Position).ChunkPos);
            _player = new PlayerController(_world, new Vector3(0, 50, 0));
            _player.Hotbar[0].Item = new ItemInstance(GameItems.Stone, 5);
            _player.Hotbar[1].Item = new ItemInstance(GameItems.Stone, 5);
            _player.Hotbar[2].Item = new ItemInstance(GameItems.Crafter, 2);
            _player.Hotbar[3].Item = new ItemInstance(GameItems.Dirt, 12);
            _player.Hotbar[4].Item = new ItemInstance(GameItems.Stone, 8);

            _rayCastContext = new WorldRayCastContext(_world);
            
            _window = new GameWindow(_tickSource, _player, _rayCastContext);
            
            _world.ChunkManager.ChunkChanged += chunk => _window.OnChunkChanged(chunk);
            _world.ChunkManager.ChunkUnloaded += chunk => _window.OnChunkUnloaded(chunk);
            _world.EntityAdded += entity => _window.OnEntityAdded(entity);
            _world.EntityRemoved += guid => _window.OnEntityRemoved(guid);
            
            _player.LoadSurroundingChunks();
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

            var hit = Raycast.Cast(_rayCastContext, _player.GetCamera(0).Ray);

            if (!_input.PrevActivate && _input.Activate)
            {
                var itemResult = _player.Hand.Item.Count > 0 ?
                    _player.Hand.Item.Type.OnActivate(new PlayerItemContext(_player.Hand.Item, _world), new ItemEvent.Activate(hit)) :
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
                    _player.Hand.Item.Type.OnPunch(new PlayerItemContext(_player.Hand.Item, _world), new ItemEvent.Punch(hit)) :
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
        }

        public static async Task Main()
        {
            var game = new Game();
            await game._window.OpenWaitClosed();
        }
    }
}