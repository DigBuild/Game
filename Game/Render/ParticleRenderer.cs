using System.Numerics;
using DigBuild.Engine.Particles;
using DigBuild.Engine.Render;
using DigBuild.Engine.Textures;
using DigBuild.Platform.Render;
using DigBuild.Platform.Util;
using DigBuild.Render.GeneratedUniforms;

namespace DigBuild.Render
{
    /// <summary>
    /// A basic particle renderer
    /// </summary>
    /// <typeparam name="TVertex"></typeparam>
    /// <typeparam name="TGpu"></typeparam>
    public class ParticleRenderer<TVertex, TGpu> : IParticleRenderer
        where TVertex : unmanaged
        where TGpu : unmanaged
    {
        private readonly NativeBufferPool _pool;
        private readonly IParticleSystem<TGpu> _particleSystem;
        private readonly Shader _vertexShader;
        private readonly Shader _fragmentShader;
        private readonly BitmapTexture _texture;
        private readonly TVertex[] _vertices;

        private readonly PooledNativeBuffer<ParticleUniform> _uniformNativeBuffer;

        private RenderPipeline<TVertex, TGpu> _pipeline = null!;
        private TextureBinding _textureBinding = null!;
        private UniformBuffer<ParticleUniform> _uniformBuffer = null!;
        private UniformBinding<ParticleUniform> _uniformBinding = null!;
        private VertexBuffer<TVertex> _vertexBuffer = null!;
        private VertexBuffer<TGpu> _instanceBuffer = null!;
        private VertexBufferWriter<TGpu> _instanceBufferWriter = null!;

        private uint _particleCount;

        public ParticleRenderer(
            NativeBufferPool pool,
            IParticleSystem<TGpu> particleSystem,
            Shader vertexShader,
            Shader fragmentShader,
            BitmapTexture texture,
            params TVertex[] vertices
        )
        {
            _pool = pool;
            _particleSystem = particleSystem;
            _vertexShader = vertexShader;
            _fragmentShader = fragmentShader;
            _texture = texture;
            _vertices = vertices;
            
            _uniformNativeBuffer = _pool.Request<ParticleUniform>();
            _uniformNativeBuffer.Add(default(ParticleUniform));
        }

        public void Initialize(RenderContext context, RenderStage stage)
        {
            VertexShader vertexShader = context.CreateVertexShader(_vertexShader.Resource)
                .WithUniform<ParticleUniform>(out var uniform);
            FragmentShader fragmentShader = context.CreateFragmentShader(_fragmentShader.Resource)
                .WithSampler(out var shaderSampler);

            TextureSampler sampler = context.CreateTextureSampler(maxFiltering: TextureFiltering.Nearest);
            Texture texture = context.CreateTexture(_texture.Bitmap);
            _textureBinding = context.CreateTextureBinding(shaderSampler, sampler, texture);

            _pipeline = context.CreatePipeline<TVertex, TGpu>(
                vertexShader, fragmentShader, stage, Topology.Triangles
            )
                .WithBlending(stage.Format.Attachments[0], BlendFactor.One, BlendFactor.One, BlendOperation.Add)
                .WithBlending(stage.Format.Attachments[1], BlendFactor.One, BlendFactor.One, BlendOperation.Add)
                .WithBlending(stage.Format.Attachments[2], BlendFactor.One, BlendFactor.One, BlendOperation.Add)
                .WithBlending(stage.Format.Attachments[3], BlendFactor.One, BlendFactor.One, BlendOperation.Add)
                .WithBlending(stage.Format.Attachments[4], BlendFactor.One, BlendFactor.One, BlendOperation.Add)
                .WithDepthTest(CompareOperation.LessOrEqual, false);

            _uniformBuffer = context.CreateUniformBuffer(_uniformNativeBuffer);
            _uniformBinding = context.CreateUniformBinding(uniform, _uniformBuffer);

            using (var vertexNativeBuffer = _pool.Request<TVertex>())
            {
                vertexNativeBuffer.Add(_vertices);
                _vertexBuffer = context.CreateVertexBuffer(vertexNativeBuffer);
            }

            _instanceBuffer = context.CreateVertexBuffer(out _instanceBufferWriter);
        }

        public void Update(float partialTick)
        {
            var nativeBuffer = _particleSystem.UpdateGpu(partialTick);
            _particleCount = nativeBuffer.Count;
            if (_particleCount > 0)
                _instanceBufferWriter.Write(nativeBuffer);
        }

        public void Draw(CommandBufferRecorder cmd, Matrix4x4 projection, Matrix4x4 flattenTransform, float partialTick)
        {
            if (_particleCount == 0)
                return;

            _uniformNativeBuffer[0].Matrix = projection;
            _uniformNativeBuffer[0].FlattenMatrix = flattenTransform;
            _uniformBuffer.Write(_uniformNativeBuffer);

            cmd.Using(_pipeline, _uniformBinding, 0);
            cmd.Using(_pipeline, _textureBinding);
            cmd.Draw(_pipeline, _vertexBuffer, _instanceBuffer);
        }
    }

    public interface IParticleUniform : IUniform<ParticleUniform>
    {
        Matrix4x4 Matrix { get; set; }
        Matrix4x4 FlattenMatrix { get; set; }
    }
}