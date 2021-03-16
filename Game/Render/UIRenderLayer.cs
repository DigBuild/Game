using DigBuild.Engine.Render;
using DigBuild.Engine.UI;
using DigBuild.Platform.Render;
using DigBuild.Render.GeneratedUniforms;

namespace DigBuild.Render
{
    public static class UIRenderLayer
    {
        public static readonly RenderLayer<UIVertex> Text = RenderLayer<UIVertex>.Create(
            UIVertex.CreateTransformer,
            ctx => GameWindow.Resources!.UiRenderStage,
            (ctx, resourceManager, renderStage) =>
            {
                var vsResource = resourceManager.GetResource(Game.Domain, "shaders/ui.vert.spv")!;
                var fsResource = resourceManager.GetResource(Game.Domain, "shaders/ui.frag.spv")!;
                VertexShader vs = ctx.CreateVertexShader(vsResource)
                    .WithUniform<SimpleUniform>(out var uniform);
                FragmentShader fs = ctx.CreateFragmentShader(fsResource)
                    .WithSampler(out var textureHandle);
                RenderPipeline<UIVertex> pipeline = ctx.CreatePipeline<UIVertex>(
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
                
                return new SimpleLayerData<UIVertex, SimpleUniform>(
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

        
        public static readonly RenderLayer<UIVertex> Ui = RenderLayer<UIVertex>.Create(
            UIVertex.CreateTransformer,
            ctx => GameWindow.Resources!.UiRenderStage,
            (ctx, resourceManager, renderStage) =>
            {
                var vsResource = resourceManager.GetResource(Game.Domain, "shaders/ui.vert.spv")!;
                var fsResource = resourceManager.GetResource(Game.Domain, "shaders/ui.frag.spv")!;
                VertexShader vs = ctx.CreateVertexShader(vsResource)
                    .WithUniform<SimpleUniform>(out var uniform);
                FragmentShader fs = ctx.CreateFragmentShader(fsResource)
                    .WithSampler(out var textureHandle);
                RenderPipeline<UIVertex> pipeline = ctx.CreatePipeline<UIVertex>(
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
                
                return new SimpleLayerData<UIVertex, SimpleUniform>(
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