using System;
using System.Numerics;
using DigBuild.Engine.Render;
using DigBuild.Platform.Render;
using DigBuild.Platform.Resource;
using DigBuild.Platform.Util;
using DigBuild.Render.Post.GeneratedUniforms;
using DigBuild.Worlds;

namespace DigBuild.Render.Post
{
    /// <summary>
    /// The world effects post processing mini-chain.
    /// </summary>
    public sealed class WorldPostProcessingEffect : IPostProcessingEffect
    {
        private Framebuffer _framebuffer = null!;
        private CommandBuffer _commandBuffer = null!;
        private UniformBuffer<WorldUniform> _uniformBuffer = null!;
        private readonly NativeBuffer<WorldUniform> _uniformBufferData = new();

        /// <summary>
        /// The projection matrix.
        /// </summary>
        public Matrix4x4 ProjectionMatrix { get; set; } = Matrix4x4.Identity;
        /// <summary>
        /// The color of the fog.
        /// </summary>
        public Vector4 FogColor { get; set; }
        /// <summary>
        /// The post-processing flags.
        /// </summary>
        public WorldPostProcessingFlags Flags { get; set; }
        /// <summary>
        /// The absolute world time.
        /// </summary>
        public ulong WorldTime { get; set; }

        /// <summary>
        /// The input world texture.
        /// </summary>
        public Texture InputWorld { get; set; } = null!;
        /// <summary>
        /// The input position texture.
        /// </summary>
        public Texture InputPosition { get; set; } = null!;
        /// <summary>
        /// The input water texture.
        /// </summary>
        public Texture InputWater { get; set; } = null!;

        /// <summary>
        /// The output texture.
        /// </summary>
        public Texture Output => _framebuffer.Get(_framebuffer.Format.Attachments[0]);

        public WorldPostProcessingEffect()
        {
            _uniformBufferData.Add(default(WorldUniform));
        }

        private void UpdateUniforms()
        {
            Matrix4x4.Invert(ProjectionMatrix, out var invMatrix);
            
            var timeOfDay = (WorldTime % World.DayDuration) / (float) World.DayDuration;
            var timeFactor = MathF.Sin(timeOfDay * 2 * MathF.PI) * 0.5f + 0.5f;
            
            _uniformBufferData[0].InverseProjection = invMatrix;
            _uniformBufferData[0].FogColor = FogColor;
            _uniformBufferData[0].Flags = (int) Flags;
            _uniformBufferData[0].TimeFactor = timeFactor;
            
            _uniformBuffer.Write(_uniformBufferData);
        }

        public void Setup(
            RenderContext context, RenderSurfaceContext surface,
            ResourceManager resourceManager, FramebufferFormat compFormat,
            VertexBuffer<Vertex2> compVertexBuffer, TextureSampler compSampler,
            NativeBufferPool bufferPool
        )
        {
            _framebuffer = context.CreateFramebuffer(compFormat, surface.Width, surface.Height);
            
            VertexShader vs = context.CreateVertexShader(resourceManager.Get<Shader>(DigBuildGame.Domain, $"effects/world.vert")!.Resource);
            FragmentShader fs = context.CreateFragmentShader(resourceManager.Get<Shader>(DigBuildGame.Domain, "effects/world.frag")!.Resource)
                .WithUniform<WorldUniform>(out var uniform)
                .WithSampler(out var samplerHandleWorld)
                .WithSampler(out var samplerHandlePosition)
                .WithSampler(out var samplerHandleWater);
            RenderPipeline<Vertex2> pipeline = context.CreatePipeline<Vertex2>(
                vs, fs, compFormat.Stages[0], Topology.Triangles
            );
            
            _uniformBuffer = context.CreateUniformBuffer<WorldUniform>();
            UpdateUniforms();

            UniformBinding<WorldUniform> uniformBinding = context.CreateUniformBinding(uniform, _uniformBuffer);

            TextureBinding textureBindingWorld = context.CreateTextureBinding(
                samplerHandleWorld, compSampler, InputWorld
            );
            TextureBinding textureBindingPosition = context.CreateTextureBinding(
                samplerHandlePosition, compSampler, InputPosition
            );
            TextureBinding textureBindingWater = context.CreateTextureBinding(
                samplerHandleWater, compSampler, InputWater
            );

            _commandBuffer = context.CreateCommandBuffer();
            using var cmd = _commandBuffer.Record(context, compFormat, bufferPool);
            cmd.SetViewportAndScissor(_framebuffer);
            cmd.Using(pipeline, uniformBinding, 0);
            cmd.Using(pipeline, textureBindingWorld);
            cmd.Using(pipeline, textureBindingPosition);
            cmd.Using(pipeline, textureBindingWater);
            cmd.Draw(pipeline, compVertexBuffer);
        }

        public void Apply(RenderContext context)
        {
            UpdateUniforms();
            context.Enqueue(_framebuffer, _commandBuffer);
        }
    }
    
    /// <summary>
    /// A world uniform.
    /// </summary>
    public interface IWorldUniform : IUniform<WorldUniform>
    {
        /// <summary>
        /// The inverse projection.
        /// </summary>
        Matrix4x4 InverseProjection { get; set; }
        /// <summary>
        /// The fog color.
        /// </summary>
        Vector4 FogColor { get; set; }
        /// <summary>
        /// The processing flags.
        /// </summary>
        int Flags { get; set; }
        /// <summary>
        /// The time factor.
        /// </summary>
        float TimeFactor { get; set; }
    }

    /// <summary>
    /// A set of world post-processing flags.
    /// </summary>
    [Flags]
    public enum WorldPostProcessingFlags : int
    {
        None = 0,
        Underwater = 1 << 0
    }
}