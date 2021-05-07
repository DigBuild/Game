using System.Collections.Generic;
using DigBuild.Engine.Events;
using DigBuild.Engine.Particles;
using DigBuild.Engine.Render;
using DigBuild.Engine.Textures;
using DigBuild.Platform.Resource;
using DigBuild.Platform.Util;
using DigBuild.Render;

namespace DigBuild.Events
{
    public sealed class ParticleSystemInitializationEvent : IEvent
    {
        private static SimplerVertex[] Vertices { get; } = new[]
        {
            new SimplerVertex(-0.5f, -0.5f, 0),
            new SimplerVertex(0.5f, -0.5f, 0),
            new SimplerVertex(0.5f, 0.5f, 0),
            new SimplerVertex(0.5f, 0.5f, 0),
            new SimplerVertex(-0.5f, 0.5f, 0),
            new SimplerVertex(-0.5f, -0.5f, 0)
        };

        private readonly NativeBufferPool _pool;
        private readonly ResourceManager _resourceManager;

        private readonly List<IParticleSystem> _systems = new();
        private readonly List<IParticleRenderer> _renderers = new();

        public IEnumerable<IParticleSystem> Systems => _systems;
        public IEnumerable<IParticleRenderer> Renderers => _renderers;

        public ParticleSystemInitializationEvent(NativeBufferPool pool, ResourceManager resourceManager)
        {
            _pool = pool;
            _resourceManager = resourceManager;
        }

        public ParticleSystem<T, TGpu> Create<T, TGpu>(
            ResourceName vertexShader,
            ResourceName fragmentShader,
            ResourceName texture
        )
            where T : unmanaged, IParticle<TGpu>
            where TGpu : unmanaged
        {
            var system = new ParticleSystem<T, TGpu>(_pool);
            _systems.Add(system);

            var renderer = new ParticleRenderer<SimplerVertex, TGpu>(
                _pool, system,
                _resourceManager.Get<Shader>(vertexShader)!,
                _resourceManager.Get<Shader>(fragmentShader)!,
                _resourceManager.Get<BitmapTexture>(texture)!,
                Vertices
            );
            _renderers.Add(renderer);

            return system;
        }
    }
}