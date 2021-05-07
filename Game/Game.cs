using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;
using System.Threading.Tasks;
using DigBuild.Client;
using DigBuild.Engine.Events;
using DigBuild.Engine.Items;
using DigBuild.Engine.Math;
using DigBuild.Engine.Particles;
using DigBuild.Engine.Physics;
using DigBuild.Engine.Worldgen;
using DigBuild.Events;
using DigBuild.Modding;
using DigBuild.Platform.Resource;
using DigBuild.Platform.Util;
using DigBuild.Players;
using DigBuild.Recipes;
using DigBuild.Registries;
using DigBuild.Worlds;

namespace DigBuild
{
    public class Game : IDisposable
    {
        public const string Domain = "digbuild";
        public const int ViewRadius = 5;
        
        public static ResourceManager ResourceManager { get; } = new(
            new ShaderCompiler("shader_out"),
            new FileSystemResourceProvider(
                new Dictionary<string, string>
                {
                    [Game.Domain] = "../../ContentMod/Resources"
                },
                true
            )
        );
        
        public static NativeBufferPool BufferPool { get; } = new();
        public static EventBus EventBus { get; } = new(); 

        public static CraftingRecipeLookup RecipeLookup { get; private set; } = null!;

        private readonly TickSource _tickSource;
        private readonly GameWindow _window;
        
        private readonly GameInput _input = new();

        private readonly World _world;
        private readonly PlayerController _player;
        private readonly WorldRayCastContext _rayCastContext;

        public Game()
        {
            ModLoader.Instance.LoadMods(); 
            foreach (var mod in ModLoader.Instance.Mods) 
                mod.AttachEvents(EventBus); 
 
            GameRegistries.Initialize(EventBus);

            var particleSystemInitializationEvent = new ParticleSystemInitializationEvent(BufferPool, ResourceManager);
            EventBus.Post(particleSystemInitializationEvent);
            
            _tickSource = new TickSource();
            _tickSource.Tick += Tick;
            
            var particleSystems = particleSystemInitializationEvent.Systems.ToImmutableList();
            var particleUpdateContext = new ParticleUpdateContext();
            _tickSource.Tick += () =>
            {
                foreach (var particleSystem in particleSystems)
                {
                    particleSystem.Update(particleUpdateContext);
                }
            };
            
            RecipeLookup = new CraftingRecipeLookup(GameRegistries.CraftingRecipes.Values);
            
            var config = Config.Load("server/config.json"); 
            
            var generator = new WorldGenerator(_tickSource, config.Worldgen.Features, 0);

            _world = new World(generator, _tickSource);
            _rayCastContext = new WorldRayCastContext(_world);

            _player = new PlayerController(_world.AddPlayer(new Vector3(0, 30, 0)));

            _player.Inventory.Hotbar[0].Item = new ItemInstance(GameRegistries.Items.GetOrNull(Domain, "campfire")!, 1);

            var particleRenderers = particleSystemInitializationEvent.Renderers.ToImmutableList();
            _window = new GameWindow(_tickSource, _player, _input, _rayCastContext, particleRenderers);
            
            _world.ChunkManager.ChunkChanged += chunk => _window.OnChunkChanged(chunk);
            _world.ChunkManager.ChunkUnloaded += chunk => _window.OnChunkUnloaded(chunk);
            _world.EntityAdded += entity => _window.OnEntityAdded(entity);
            _world.EntityRemoved += guid => _window.OnEntityRemoved(guid);

            _world.BlockChanged += pos =>
            {
                ChunkBlockLight.Update(_world, pos);
                foreach (var direction in Directions.All)
                    ChunkBlockLight.Update(_world, pos.Offset(direction));
            };
        }

        public void Dispose()
        {
            _world.Dispose();
        }

        private void Tick()
        {
            _input.Update();
            
            _player.UpdateMovement(_input);
            _player.UpdateHotbar(_input);
            
            var hit = Raycast.Cast(_rayCastContext, _player.GetCamera(0).Ray);
            _player.UpdateInteraction(_input, hit);
        }

        public static async Task Main()
        {
            var game = new Game();
            await game._window.OpenWaitClosed();
            game.Dispose();
        }
    }
}