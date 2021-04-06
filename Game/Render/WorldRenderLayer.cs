using DigBuild.Client;
using DigBuild.Engine.Render;
using DigBuild.Platform.Render;
using DigBuild.Render.GeneratedUniforms;

namespace DigBuild.Render
{
    public static class WorldRenderLayer
    {
        public static readonly RenderLayer<SimpleVertex> Opaque = RenderLayer<SimpleVertex>.Create(
            SimpleVertex.CreateTransformer,
            ctx => GameWindow.Resources!.MainRenderStage,
            (ctx, resourceManager, renderStage) =>
            {
                var vsResource = resourceManager.GetResource(Game.Domain, "shaders/world/opaque.vert.spv")!;
                var fsResource = resourceManager.GetResource(Game.Domain, "shaders/world/opaque.frag.spv")!;
                VertexShader vs = ctx.CreateVertexShader(vsResource)
                    .WithUniform<SimpleUniform>(out var uniform);
                FragmentShader fs = ctx.CreateFragmentShader(fsResource)
                    .WithSampler(out var textureHandle);
                RenderPipeline<SimpleVertex> pipeline = ctx.CreatePipeline<SimpleVertex>(
                        vs, fs,
                        renderStage, Topology.Triangles
                    )
                    .WithDepthTest(CompareOperation.LessOrEqual, true);

                // Create spritesheet stuff
                TextureSampler sampler = ctx.CreateTextureSampler(
                    TextureFiltering.Linear, TextureFiltering.Nearest
                );
                TextureBinding blockTextureBinding = ctx.CreateTextureBinding(
                    textureHandle,
                    sampler,
                    GameWindow.Resources!.BlockTexture
                );
                
                return new SimpleLayerData<SimpleVertex, SimpleUniform>(
                    pipeline, blockTextureBinding, uniform,
                    mat => new SimpleUniform(){Matrix = mat}
                );
            },
            data => data.Pipeline,
            (data, pool) => data.CreateUniforms(pool),
            (data, cmd) => cmd.Using(data.Pipeline, data.TextureBinding));
        
        public static readonly RenderLayer<SimpleVertex> Translucent = RenderLayer<SimpleVertex>.Create(
            SimpleVertex.CreateTransformer,
            ctx => GameWindow.Resources!.MainRenderStage,
            (ctx, resourceManager, renderStage) =>
            {
                var vsResource = resourceManager.GetResource(Game.Domain, "shaders/world/opaque.vert.spv")!;
                var fsResource = resourceManager.GetResource(Game.Domain, "shaders/world/opaque.frag.spv")!;
                VertexShader vs = ctx.CreateVertexShader(vsResource)
                    .WithUniform<SimpleUniform>(out var uniform);
                FragmentShader fs = ctx.CreateFragmentShader(fsResource)
                    .WithSampler(out var textureHandle);
                RenderPipeline<SimpleVertex> pipeline = ctx.CreatePipeline<SimpleVertex>(
                        vs, fs,
                        renderStage, Topology.Triangles
                    )
                    .WithDepthTest(CompareOperation.LessOrEqual, true)
                    .WithStandardBlending(GameWindow.Resources!.WorldFramebuffer.Format.Attachments[0])
                    .WithStandardBlending(GameWindow.Resources!.WorldFramebuffer.Format.Attachments[1]);

                // Create spritesheet stuff
                TextureSampler sampler = ctx.CreateTextureSampler(
                    TextureFiltering.Linear, TextureFiltering.Nearest
                );
                TextureBinding blockTextureBinding = ctx.CreateTextureBinding(
                    textureHandle,
                    sampler,
                    GameWindow.Resources!.BlockTexture
                );
                
                return new SimpleLayerData<SimpleVertex, SimpleUniform>(
                    pipeline, blockTextureBinding, uniform,
                    mat => new SimpleUniform(){Matrix = mat}
                );
            },
            data => data.Pipeline,
            (data, pool) => data.CreateUniforms(pool),
            (data, cmd) => cmd.Using(data.Pipeline, data.TextureBinding));
    }
}