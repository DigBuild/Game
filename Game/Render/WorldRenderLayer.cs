using DigBuild.Controller;
using DigBuild.Engine.Render;
using DigBuild.Platform.Render;
using DigBuild.Render.GeneratedUniforms;

namespace DigBuild.Render
{
    public static class WorldRenderLayer
    {
        public static readonly RenderLayer<WorldVertex> Opaque = RenderLayer<WorldVertex>.Create(
            WorldVertex.CreateTransformer,
            ctx => GameplayController.RenderStage,
            (ctx, resourceManager, renderStage) =>
            {
                var vsResource = resourceManager.Get<Shader>(DigBuildGame.Domain, "world/base.vert")!;
                var fsResource = resourceManager.Get<Shader>(DigBuildGame.Domain, "world/opaque.frag")!;
                VertexShader vs = ctx.CreateVertexShader(vsResource.Resource)
                    .WithUniform<SimpleUniform>(out var uniform);
                FragmentShader fs = ctx.CreateFragmentShader(fsResource.Resource)
                    .WithSampler(out var textureHandle);
                RenderPipeline<WorldVertex> pipeline = ctx.CreatePipeline<WorldVertex>(
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
                    GameplayController.TextureSheet
                );
                
                return new SimpleLayerData<WorldVertex, SimpleUniform>(
                    pipeline, blockTextureBinding, uniform,
                    mat => new SimpleUniform(){Matrix = mat}
                );
            },
            data => data.Pipeline,
            (data, pool) => data.CreateUniforms(pool),
            (data, cmd) => cmd.Using(data.Pipeline, data.TextureBinding));

        
        public static readonly RenderLayer<WorldVertex> Cutout = RenderLayer<WorldVertex>.Create(
            WorldVertex.CreateTransformer,
            ctx => GameplayController.RenderStage,
            (ctx, resourceManager, renderStage) =>
            {
                var vsResource = resourceManager.Get<Shader>(DigBuildGame.Domain, "world/base.vert")!;
                var fsResource = resourceManager.Get<Shader>(DigBuildGame.Domain, "world/cutout.frag")!;
                VertexShader vs = ctx.CreateVertexShader(vsResource.Resource)
                    .WithUniform<SimpleUniform>(out var uniform);
                FragmentShader fs = ctx.CreateFragmentShader(fsResource.Resource)
                    .WithSampler(out var textureHandle);
                RenderPipeline<WorldVertex> pipeline = ctx.CreatePipeline<WorldVertex>(
                        vs, fs,
                        renderStage, Topology.Triangles
                    )
                    .WithDepthTest(CompareOperation.LessOrEqual, true)
                    .WithStandardBlending(renderStage.Format.Attachments[0])
                    .WithStandardBlending(renderStage.Format.Attachments[1]);

                // Create spritesheet stuff
                TextureSampler sampler = ctx.CreateTextureSampler(
                    TextureFiltering.Linear, TextureFiltering.Nearest
                );
                TextureBinding blockTextureBinding = ctx.CreateTextureBinding(
                    textureHandle,
                    sampler,
                    GameplayController.TextureSheet
                );
                
                return new SimpleLayerData<WorldVertex, SimpleUniform>(
                    pipeline, blockTextureBinding, uniform,
                    mat => new SimpleUniform(){Matrix = mat}
                );
            },
            data => data.Pipeline,
            (data, pool) => data.CreateUniforms(pool),
            (data, cmd) => cmd.Using(data.Pipeline, data.TextureBinding));
        
        public static readonly RenderLayer<WorldVertex> Translucent = RenderLayer<WorldVertex>.Create(
            WorldVertex.CreateTransformer,
            ctx => GameplayController.RenderStage,
            (ctx, resourceManager, renderStage) =>
            {
                var vsResource = resourceManager.Get<Shader>(DigBuildGame.Domain, "world/base.vert")!;
                var fsResource = resourceManager.Get<Shader>(DigBuildGame.Domain, "world/translucent.frag")!;
                VertexShader vs = ctx.CreateVertexShader(vsResource.Resource)
                    .WithUniform<SimpleUniform>(out var uniform);
                FragmentShader fs = ctx.CreateFragmentShader(fsResource.Resource)
                    .WithSampler(out var textureHandle);
                RenderPipeline<WorldVertex> pipeline = ctx.CreatePipeline<WorldVertex>(
                        vs, fs,
                        renderStage, Topology.Triangles
                    )
                    .WithDepthTest(CompareOperation.LessOrEqual, true)
                    .WithStandardBlending(renderStage.Format.Attachments[0])
                    .WithStandardBlending(renderStage.Format.Attachments[1]);

                // Create spritesheet stuff
                TextureSampler sampler = ctx.CreateTextureSampler(
                    TextureFiltering.Linear, TextureFiltering.Nearest
                );
                TextureBinding blockTextureBinding = ctx.CreateTextureBinding(
                    textureHandle,
                    sampler,
                    GameplayController.TextureSheet
                );
                
                return new SimpleLayerData<WorldVertex, SimpleUniform>(
                    pipeline, blockTextureBinding, uniform,
                    mat => new SimpleUniform(){Matrix = mat}
                );
            },
            data => data.Pipeline,
            (data, pool) => data.CreateUniforms(pool),
            (data, cmd) => cmd.Using(data.Pipeline, data.TextureBinding));
    }
}