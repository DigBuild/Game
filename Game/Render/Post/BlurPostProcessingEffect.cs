using DigBuild.Engine.Render;
using DigBuild.Platform.Render;
using DigBuild.Platform.Resource;
using DigBuild.Platform.Util;
using DigBuild.Render.Post.GeneratedUniforms;

namespace DigBuild.Render.Post
{
    /// <summary>
    /// A basic blur post-processing effect.
    /// </summary>
    public sealed class BlurPostProcessingEffect : IPostProcessingEffect
    {
        private readonly BlurDirection _direction;
        private readonly uint _sizeDivider;
        
        private Framebuffer _framebuffer = null!;
        private CommandBuffer _commandBuffer = null!;

        /// <summary>
        /// The input texture.
        /// </summary>
        public Texture Input { get; set; } = null!;
        /// <summary>
        /// The output texture.
        /// </summary>
        public Texture Output => _framebuffer.Get(_framebuffer.Format.Attachments[0]);

        public BlurPostProcessingEffect(BlurDirection direction, uint sizeDivider)
        {
            _direction = direction;
            _sizeDivider = sizeDivider;
        }

        public void Setup(
            RenderContext context, RenderSurfaceContext surface,
            ResourceManager resourceManager, FramebufferFormat compFormat,
            VertexBuffer<Vertex2> compVertexBuffer, TextureSampler compSampler,
            NativeBufferPool bufferPool
        )
        {
            _framebuffer = context.CreateFramebuffer(
                compFormat,
                surface.Width / _sizeDivider,
                surface.Height / _sizeDivider
            );

            var vert = _direction == BlurDirection.Vertical ? "vblur" : "hblur";
            
            VertexShader vs = context.CreateVertexShader(resourceManager.Get<Shader>(DigBuildGame.Domain, $"effects/{vert}.vert")!.Resource)
                .WithUniform<PixelSizeUniform>(out var uniform);
            FragmentShader fs = context.CreateFragmentShader(resourceManager.Get<Shader>(DigBuildGame.Domain, "effects/blur.frag")!.Resource)
                .WithSampler(out var samplerHandle);
            RenderPipeline<Vertex2> pipeline = context.CreatePipeline<Vertex2>(
                vs, fs, compFormat.Stages[0], Topology.Triangles
            );

            using var pixelSizeData = new NativeBuffer<PixelSizeUniform>
            {
                new PixelSizeUniform {PixelSize = 1f / _framebuffer.Width}
            };
            UniformBuffer<PixelSizeUniform> uniformBuffer = context.CreateUniformBuffer(pixelSizeData);
            UniformBinding<PixelSizeUniform> uniformBinding = context.CreateUniformBinding(uniform, uniformBuffer);

            TextureBinding textureBinding = context.CreateTextureBinding(
                samplerHandle, compSampler, Input
            );

            _commandBuffer = context.CreateCommandBuffer();
            using var cmd = _commandBuffer.Record(context, compFormat, bufferPool);
            cmd.SetViewportAndScissor(_framebuffer);
            cmd.Using(pipeline, uniformBinding, 0);
            cmd.Using(pipeline, textureBinding);
            cmd.Draw(pipeline, compVertexBuffer);
        }

        public void Apply(RenderContext context)
        {
            context.Enqueue(_framebuffer, _commandBuffer);
        }
    }

    /// <summary>
    /// A blur direction.
    /// </summary>
    public enum BlurDirection
    {
        Vertical,
        Horizontal,
    }

    /// <summary>
    /// A pixel size uniform.
    /// </summary>
    public interface IPixelSizeUniform : IUniform<PixelSizeUniform>
    {
        /// <summary>
        /// The size multiplier for a pixel.
        /// </summary>
        float PixelSize { get; set; }
    }
}