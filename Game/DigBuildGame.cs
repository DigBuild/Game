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
    public sealed class DigBuildGame : IDisposable
    {
        public const string Domain = "digbuild";
        public const ushort ViewRadius = 4;

        public static DigBuildGame Instance { get; internal set; } = null!;
        
        public ResourceManager ResourceManager { get; }

        public NativeBufferPool BufferPool { get; } = new();
        public EventBus EventBus { get; }

        public AudioManager AudioManager { get; }
        public ModelManager ModelManager { get; } = new();

        public IGameController Controller { get; private set; }
        public TickSource TickSource { get; } = new();
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

            TickSource.Tick += () => Controller.Tick();
            Window = new GameWindow(this);
            Controller = new GameplayController(this);
        }

        public void Dispose()
        {
            Controller.Dispose();
        }
        
        public void Start()
        {
            if (TickSource.Running)
                throw new InvalidOperationException("Game has already started.");

            TickSource.Start();
            Window.Open().Wait();
        }

        public void Exit()
        {
            if (!TickSource.Running)
                throw new InvalidOperationException("Game has already exited.");

            TickSource.Stop();
            Window.Close().Wait();
        }

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