using System;
using System.Collections.Generic;
using System.Linq;
using DigBuild.Audio;
using DigBuild.Controller;
using DigBuild.Engine.Events;
using DigBuild.Entities.Models;
using DigBuild.Events;
using DigBuild.Modding;
using DigBuild.Platform.Resource;
using DigBuild.Platform.Util;
using DigBuild.Registries;
using DigBuild.Render;
using GameWindow = DigBuild.Render.GameWindow;

namespace DigBuild
{
    /// <summary>
    /// The main game class.
    /// </summary>
    public sealed class DigBuildGame : IDisposable
    {
        /// <summary>
        /// The game's domain.
        /// </summary>
        public const string Domain = "digbuild";

        /// <summary>
        /// The render view radius.
        /// </summary>
        public const ushort ViewRadius = 4;

        /// <summary>
        /// The instance of the game.
        /// </summary>
        public static DigBuildGame Instance { get; internal set; } = null!;
        
        /// <summary>
        /// The resoruce manager.
        /// </summary>
        public ResourceManager ResourceManager { get; }

        /// <summary>
        /// The shared native buffer pool.
        /// </summary>
        public NativeBufferPool BufferPool { get; } = new();

        /// <summary>
        /// The event bus.
        /// </summary>
        public EventBus EventBus { get; }
        
        /// <summary>
        /// The audio manager.
        /// </summary>
        public AudioManager AudioManager { get; }

        /// <summary>
        /// The model manager.
        /// </summary>
        public ModelManager ModelManager { get; } = new();

        /// <summary>
        /// The active game controller.
        /// </summary>
        public IGameController Controller { get; private set; }

        /// <summary>
        /// The tick source.
        /// </summary>
        public TickSource TickSource { get; } = new();

        /// <summary>
        /// The game window.
        /// </summary>
        public GameWindow Window { get; }
        
        internal DigBuildGame(EventBus eventBus)
        {
            var modResources = ModLoader.Instance.Mods.Select(mod => mod.Resources);
            
            var resourceProviders = new List<IResourceProvider>
            {
                new ShaderCompiler("shader_out")
            };
            resourceProviders.AddRange(modResources);

            ResourceManager = new ResourceManager(resourceProviders);

            foreach (var systemData in GameRegistries.ParticleSystems.Values)
                systemData.InitializeRenderer(BufferPool, ResourceManager);

            EventBus = eventBus;
            EventBus.Subscribe<ModelsBakedEvent>(OnModelsBaked);

            AudioManager = new AudioManager(ResourceManager);
            
            TickSource.HighPriorityTick += () => Controller.SystemTick();
            TickSource.Tick += () => Controller.Tick();
            Window = new GameWindow(this);
            Controller = new GameplayController(this);
        }

        public void Dispose()
        {
            Controller.Dispose();
        }
        
        /// <summary>
        /// Starts the game.
        /// </summary>
        public void Start()
        {
            if (TickSource.Running)
                throw new InvalidOperationException("Game has already started.");

            TickSource.Start();
            Window.Open().Wait();
        }

        /// <summary>
        /// Exits the game.
        /// </summary>
        public void Exit()
        {
            if (!TickSource.Running)
                throw new InvalidOperationException("Game has already exited.");

            TickSource.Stop();
            Window.Close().Wait();
        }

        /// <summary>
        /// Waits for everything within the game to have exited safely.
        /// </summary>
        public void Await()
        {
            Window.Surface.Closed.Wait();
            AudioManager.Stop();
            if (TickSource.Running)
                TickSource.Stop();
            TickSource.Await();
        }

        private void OnModelsBaked(ModelsBakedEvent evt)
        {
            evt.ModelManager[GameEntities.Item] = new ItemEntityModel(evt.ModelManager.ItemModels);
        }
    }
}