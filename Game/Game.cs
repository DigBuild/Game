using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using DigBuildEngine.Math;
using DigBuildEngine.Reg;
using DigBuildEngine.Voxel;
using DigBuildEngine.Worldgen;
using DigBuild.Blocks;
using DigBuild.Worldgen;
using DigBuildPlatformCS.Resource;

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

        public Game()
        {
            var registryManager = new RegistryManager();
            CreateRegistries(registryManager);
            registryManager.BuildAll();
            
            var features = new List<IWorldgenFeature>
            {
                WorldgenFeatures.Terrain,
                WorldgenFeatures.Water
            };
            var generator = new WorldGenerator(features, 0);
            _world = new World(generator);
            _player = new PlayerController(_world, new Vector3(0, 15, 0));
            
            _tickManager = new TickManager(Tick);
            _window = new GameWindow(_tickManager, _player);
        }

        private void CreateRegistries(RegistryManager manager)
        {
            var blocks = manager.CreateRegistryOf<Block>(new ResourceName(Domain, "blocks"));
            blocks.Building += GameBlocks.Register;

            var worldgenAttributes = manager.CreateRegistryOf<IWorldgenAttribute>(new ResourceName(Domain, "worldgen_attributes"));
            worldgenAttributes.Building += WorldgenAttributes.Register;

            var worldgenFeatures = manager.CreateRegistryOf<IWorldgenFeature>(new ResourceName(Domain, "worldgen_features"));
            worldgenFeatures.Building += WorldgenFeatures.Register;
        }
        
        private void Tick()
        {
            _input.Update();
            
            _player.UpdateRotation(_input.PitchDelta, _input.YawDelta);
            _player.ApplyMotion(_input.ForwardDelta, _input.SidewaysDelta);
            if (_input.Jump)
                _player.Jump(_input.ForwardDelta);
            _player.Move();
                
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