using DigBuild.Engine.Render;
using DigBuild.Engine.Ui;
using DigBuild.Platform.Render;
using DigBuild.Render.GeneratedUniforms;

namespace DigBuild.Render
{
    public static class UiRenderLayer
    {
        public static readonly RenderLayer<UiVertex> Text = RenderLayer<UiVertex>.Create(
            UiVertex.CreateTransformer,
            ctx => GameWindow.Resources!.UiRenderStage,
            (ctx, resourceManager, renderStage) =>
            {
                var vsResource = resourceManager.GetResource(Game.Domain, "shaders/ui.vert.spv")!;
                var fsResource = resourceManager.GetResource(Game.Domain, "shaders/ui.frag.spv")!;
                VertexShader vs = ctx.CreateVertexShader(vsResource)
                    .WithUniform<SimpleUniform>(out var uniform);
                FragmentShader fs = ctx.CreateFragmentShader(fsResource)
                    .WithSampler(out var textureHandle);
                RenderPipeline<UiVertex> pipeline = ctx.CreatePipeline<UiVertex>(
                    vs, fs,
                    renderStage, Topology.Triangles
                ).WithStandardBlending(GameWindow.Resources!.UiFramebuffer.Format.Attachments[0]);

                // Create spritesheet stuff
                TextureSampler sampler = ctx.CreateTextureSampler(
                    TextureFiltering.Nearest, TextureFiltering.Nearest
                );
                TextureBinding textureBinding = ctx.CreateTextureBinding(
                    textureHandle,
                    sampler,
                    GameWindow.Resources!.FontTexture
                );
                
                return new SimpleLayerData<UiVertex, SimpleUniform>(
                    pipeline, textureBinding, uniform,
                    mat => new SimpleUniform(){Matrix = mat}
                );
            },
            data => data.Pipeline,
            (data, pool) => data.CreateUniforms(pool),
            (data, cmd) =>
            {
                cmd.Using(data.Pipeline, data.TextureBinding);
            });

        
        public static readonly RenderLayer<UiVertex> Ui = RenderLayer<UiVertex>.Create(
            UiVertex.CreateTransformer,
            ctx => GameWindow.Resources!.UiRenderStage,
            (ctx, resourceManager, renderStage) =>
            {
                var vsResource = resourceManager.GetResource(Game.Domain, "shaders/ui.vert.spv")!;
                var fsResource = resourceManager.GetResource(Game.Domain, "shaders/ui.frag.spv")!;
                VertexShader vs = ctx.CreateVertexShader(vsResource)
                    .WithUniform<SimpleUniform>(out var uniform);
                FragmentShader fs = ctx.CreateFragmentShader(fsResource)
                    .WithSampler(out var textureHandle);
                RenderPipeline<UiVertex> pipeline = ctx.CreatePipeline<UiVertex>(
                    vs, fs,
                    renderStage, Topology.Triangles
                ).WithStandardBlending(GameWindow.Resources!.UiFramebuffer.Format.Attachments[0]);

                // Create spritesheet stuff
                TextureSampler sampler = ctx.CreateTextureSampler(
                    TextureFiltering.Nearest, TextureFiltering.Nearest
                );
                TextureBinding textureBinding = ctx.CreateTextureBinding(
                    textureHandle,
                    sampler,
                    GameWindow.Resources!.UiTexture
                );
                
                return new SimpleLayerData<UiVertex, SimpleUniform>(
                    pipeline, textureBinding, uniform,
                    mat => new SimpleUniform(){Matrix = mat}
                );
            },
            data => data.Pipeline,
            (data, pool) => data.CreateUniforms(pool),
            (data, cmd) =>
            {
                cmd.Using(data.Pipeline, data.TextureBinding);
            });
    }
}