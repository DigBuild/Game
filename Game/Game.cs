using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using DigBuildEngine.Math;
using DigBuildEngine.Reg;
using DigBuildEngine.Voxel;
using DigBuildEngine.Worldgen;
using DigBuild.Blocks;
using DigBuild.Worldgen;
using DigBuildPlatformCS;
using DigBuildPlatformCS.Input;
using DigBuildPlatformCS.Resource;

namespace DigBuild
{
    public struct Input
    {
        public float PitchDelta, YawDelta, ForwardDelta, SidewaysDelta;
        public bool Jump;

        public void Reset()
        {
            PitchDelta = YawDelta = ForwardDelta = SidewaysDelta = 0;
            Jump = false;
        }
    }

    public class Game
    {
        public const string Domain = "digbuild";

        private World _world = null!;
        private PlayerController _player = null!;
        private readonly HashSet<ChunkPos> _visitedChunks = new();

        private readonly TickManager _tickManager;
        private readonly GameWindow _window;

        private Input _input;

        public Game()
        {
            _tickManager = new TickManager(Tick);
            
            var registryManager = new RegistryManager();
            CreateRegistries(registryManager);
            registryManager.BuildAll();

            _window = new GameWindow(_tickManager, _player);
        }

        public async Task OpenWaitClosed()
        {
            await _window.OpenWaitClosed();
        }

        public void CreateRegistries(RegistryManager manager)
        {
            var blocks = manager.CreateRegistryOf<Block>(new ResourceName(Domain, "blocks"));
            blocks.Building += GameBlocks.Register;

            var worldgenAttributes = manager.CreateRegistryOf<IWorldgenAttribute>(new ResourceName(Domain, "worldgen_attributes"));
            worldgenAttributes.Building += WorldgenAttributes.Register;

            var worldgenFeatures = manager.CreateRegistryOf<IWorldgenFeature>(new ResourceName(Domain, "worldgen_features"));
            worldgenFeatures.Building += WorldgenFeatures.Register;
            worldgenFeatures.Built += OnWorldgenFeaturesReady;
        }

        private void OnWorldgenFeaturesReady(Registry<IWorldgenFeature> registry)
        {
            var features = new List<IWorldgenFeature>
            {
                WorldgenFeatures.Terrain,
                WorldgenFeatures.Water
            };
            var generator = new WorldGenerator(features, 0);

            _world = new World(generator);

            _player = new PlayerController(_world, new Vector3(0, 15, 0));
        }
        
        private Controller? _controller;

        private void Tick()
        {
            Platform.InputContext.Update();
            _controller ??= Platform.InputContext.Controllers.FirstOrDefault();
            if (_controller == null)
                return;
            
            _input.PitchDelta += Bias(_controller.Joysticks[3]);
            _input.YawDelta += Bias(_controller.Joysticks[2]);
            _input.ForwardDelta -= Bias(_controller.Joysticks[1]);
            _input.SidewaysDelta -= -Bias(_controller.Joysticks[0]);
            _input.Jump = _controller.Buttons[5];

            // var start = DateTime.Now.Millisecond;

            _player.UpdateRotation(_input.PitchDelta, _input.YawDelta);
            _player.ApplyMotion(_input.ForwardDelta, _input.SidewaysDelta);
            if (_input.Jump)
                _player.Jump(_input.ForwardDelta);
            _player.Move();
                
            const int range = 5;
            const int rangeY = 3;

            var chunkPos = new BlockPos(_player.Position).ChunkPos;
            for (int x = -range; x < range; x++)
            {
                for (int z = -range; z < range; z++)
                {
                    for (int y = 0; y < rangeY; y++)
                    {
                        var p = new ChunkPos(chunkPos.X + x, y, chunkPos.Z + z);
                        if (_visitedChunks.Add(p)) // Only visit new chunks
                            _window.OnChunkChanged(_world.GetChunk(p)!);
                    }
                }
            }

            _input.Reset();

            // Console.WriteLine($"Elapsed: {DateTime.Now.Millisecond - start}ms");
        }

        private static float Bias(float value)
        {
            if (Math.Abs(value) < 0.1F) return 0;
            return value;
        }

        public static async Task Main()
        {
            var game = new Game();
            await game.OpenWaitClosed();
        }
    }
}