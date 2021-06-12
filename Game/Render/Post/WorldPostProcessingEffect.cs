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
    public sealed class WorldPostProcessingEffect : IPostProcessingEffect
    {
        private Framebuffer _framebuffer = null!;
        private CommandBuffer _commandBuffer = null!;
        private UniformBuffer<WorldUniform> _uniformBuffer = null!;
        private readonly NativeBuffer<WorldUniform> _uniformBufferData = new();

        public Matrix4x4 ProjectionMatrix { get; set; } = Matrix4x4.Identity;
        public Vector4 FogColor { get; set; }
        public WorldPostProcessingFlags Flags { get; set; }
        public ulong WorldTime { get; set; }

        public Texture InputWorld { get; set; } = null!;
        public Texture InputPosition { get; set; } = null!;
        public Texture InputWater { get; set; } = null!;

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
    
    public interface IWorldUniform : IUniform<WorldUniform>
    {
        Matrix4x4 InverseProjection { get; set; }
        Vector4 FogColor { get; set; }
        int Flags { get; set; }
        float TimeFactor { get; set; }
    }

    [Flags]
    public enum WorldPostProcessingFlags : int
    {
        None = 0,
        Underwater = 1 << 0
    }
}