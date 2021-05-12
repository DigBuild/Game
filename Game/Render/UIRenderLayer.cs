using DigBuild.Controller;
using DigBuild.Engine.Render;
using DigBuild.Engine.Ui;
using DigBuild.Platform.Render;
using DigBuild.Render.GeneratedUniforms;
using DigBuild.Ui;

namespace DigBuild.Render
{
    public static class UiRenderLayer
    {
        public static readonly RenderLayer<UiVertex> Text = RenderLayer<UiVertex>.Create(
            UiVertex.CreateTransformer,
            ctx => GameplayController.RenderStage,
            (ctx, resourceManager, renderStage) =>
            {
                var vsResource = resourceManager.Get<Shader>(DigBuildGame.Domain, "ui.vert")!;
                var fsResource = resourceManager.Get<Shader>(DigBuildGame.Domain, "ui.frag")!;
                VertexShader vs = ctx.CreateVertexShader(vsResource.Resource)
                    .WithUniform<SimpleUniform>(out var uniform);
                FragmentShader fs = ctx.CreateFragmentShader(fsResource.Resource)
                    .WithSampler(out var textureHandle);
                RenderPipeline<UiVertex> pipeline = ctx.CreatePipeline<UiVertex>(
                    vs, fs,
                    renderStage, Topology.Triangles
                )
                    .WithStandardBlending(renderStage.Format.Attachments[0])
                    .WithStandardBlending(renderStage.Format.Attachments[1]);

                // Create spritesheet stuff
                TextureSampler sampler = ctx.CreateTextureSampler(
                    TextureFiltering.Nearest, TextureFiltering.Nearest
                );
                TextureBinding textureBinding = ctx.CreateTextureBinding(
                    textureHandle,
                    sampler,
                    UiManager.FontTexture
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
        ctx => GameplayController.RenderStage,
        (ctx, resourceManager, renderStage) =>
        {
            var vsResource = resourceManager.Get<Shader>(DigBuildGame.Domain, "ui.vert")!;
            var fsResource = resourceManager.Get<Shader>(DigBuildGame.Domain, "ui.frag")!;
            VertexShader vs = ctx.CreateVertexShader(vsResource.Resource)
                .WithUniform<SimpleUniform>(out var uniform);
            FragmentShader fs = ctx.CreateFragmentShader(fsResource.Resource)
                .WithSampler(out var textureHandle);
            RenderPipeline<UiVertex> pipeline = ctx.CreatePipeline<UiVertex>(
                vs, fs,
                renderStage, Topology.Triangles
            )
                .WithStandardBlending(renderStage.Format.Attachments[0])
                .WithStandardBlending(renderStage.Format.Attachments[1]);
        
            // Create spritesheet stuff
            TextureSampler sampler = ctx.CreateTextureSampler(
                TextureFiltering.Nearest, TextureFiltering.Nearest
            );
            TextureBinding textureBinding = ctx.CreateTextureBinding(
                textureHandle,
                sampler,
                UiManager.UiTexture
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