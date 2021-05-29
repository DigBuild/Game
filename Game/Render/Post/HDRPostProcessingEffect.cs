using DigBuild.Engine.Render;
using DigBuild.Platform.Render;
using DigBuild.Platform.Resource;
using DigBuild.Platform.Util;

namespace DigBuild.Render.Post
{
    public sealed class HDRPostProcessingEffect : IPostProcessingEffect
    {
        private Framebuffer _framebuffer = null!;
        private CommandBuffer _commandBuffer = null!;

        public Texture Input { get; set; } = null!;
        public Texture Output => _framebuffer.Get(_framebuffer.Format.Attachments[0]);
        
        public void Setup(
            RenderContext context, RenderSurfaceContext surface,
            ResourceManager resourceManager, FramebufferFormat compFormat,
            VertexBuffer<Vertex2> compVertexBuffer, TextureSampler compSampler,
            NativeBufferPool bufferPool
        )
        {
            _framebuffer = context.CreateFramebuffer(compFormat, surface.Width, surface.Height);
            
            VertexShader vs = context.CreateVertexShader(resourceManager.Get<Shader>(DigBuildGame.Domain, $"effects/hdr.vert")!.Resource);
            FragmentShader fs = context.CreateFragmentShader(resourceManager.Get<Shader>(DigBuildGame.Domain, "effects/hdr.frag")!.Resource)
                .WithSampler(out var samplerHandle);
            RenderPipeline<Vertex2> pipeline = context.CreatePipeline<Vertex2>(
                vs, fs, compFormat.Stages[0], Topology.Triangles
            );
            TextureBinding textureBinding = context.CreateTextureBinding(
                samplerHandle, compSampler, Input
            );

            _commandBuffer = context.CreateCommandBuffer();
            using var cmd = _commandBuffer.Record(context, compFormat, bufferPool);
            cmd.SetViewportAndScissor(_framebuffer);
            cmd.Using(pipeline, textureBinding);
            cmd.Draw(pipeline, compVertexBuffer);
        }

        public void Apply(RenderContext context)
        {
            context.Enqueue(_framebuffer, _commandBuffer);
        }
    }
}