using DigBuild.Engine.Particles;
using DigBuild.Engine.Registries;
using DigBuild.Engine.Render;
using DigBuild.Engine.Textures;
using DigBuild.Platform.Resource;
using DigBuild.Platform.Util;
using DigBuild.Render;

namespace DigBuild.Registries
{
    public interface IParticleSystemData
    {
        IParticleSystem System { get; }
        IParticleRenderer Renderer { get; }

        internal void InitializeRenderer(NativeBufferPool pool, ResourceManager resourceManager);
    }

    public sealed class ParticleSystemData<T, TGpu> : IParticleSystemData
        where T : unmanaged, IParticle<TGpu>
        where TGpu : unmanaged
    {
        private readonly ParticleSystem<T, TGpu> _system;
        private readonly ResourceName _vertexShader;
        private readonly ResourceName _fragmentShader;
        private readonly ResourceName _texture;

        public IParticleSystem System => _system;

        public IParticleRenderer Renderer { get; private set; } = null!;

        internal ParticleSystemData(
            ParticleSystem<T, TGpu> system,
            ResourceName vertexShader,
            ResourceName fragmentShader,
            ResourceName texture
        )
        {
            _system = system;
            _vertexShader = vertexShader;
            _fragmentShader = fragmentShader;
            _texture = texture;
        }

        void IParticleSystemData.InitializeRenderer(NativeBufferPool pool, ResourceManager resourceManager)
        {
            Renderer = new ParticleRenderer<Vertex3, TGpu>(
                pool, _system,
                resourceManager.Get<Shader>(_vertexShader)!,
                resourceManager.Get<Shader>(_fragmentShader)!,
                resourceManager.Get<BitmapTexture>(_texture)!,
                ParticleSystemRegistryBuilderExtensions.Vertices
            );
        }
    }

    public static class ParticleSystemRegistryBuilderExtensions
    {
        private static readonly NativeBufferPool Pool = new(); // TODO: Ideally unify?
        internal static Vertex3[] Vertices { get; } = {
            new(-0.5f, -0.5f, 0),
            new(0.5f, -0.5f, 0),
            new(0.5f, 0.5f, 0),
            new(0.5f, 0.5f, 0),
            new(-0.5f, 0.5f, 0),
            new(-0.5f, -0.5f, 0)
        };

        public static ParticleSystem<T, TGpu> Create<T, TGpu>(
            this IRegistryBuilder<IParticleSystemData> registry,
            string domain, string path, 
            ResourceName vertexShader,
            ResourceName fragmentShader,
            ResourceName texture
        )
            where T : unmanaged, IParticle<TGpu>
            where TGpu : unmanaged
        {
            var system = new ParticleSystem<T, TGpu>(Pool);
            registry.Add(new ResourceName(domain, path), new ParticleSystemData<T, TGpu>(system, vertexShader, fragmentShader, texture));
            return system;
        }
    }
}