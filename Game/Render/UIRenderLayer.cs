using System;
using System.Numerics;
using DigBuild.Engine.Render;
using DigBuild.Engine.UI;
using DigBuild.Platform.Render;
using DigBuild.Platform.Util;
using DigBuild.Render.GeneratedUniforms;

namespace DigBuild.Render
{
    public static class UIRenderLayer
    {
        public static readonly RenderLayer<TextVertex> Text = RenderLayer<TextVertex>.Create(
            TextVertex.CreateTransformer,
            ctx => GameWindow.Resources!.UiRenderStage,
            (ctx, resourceManager, renderStage) =>
            {
                var vsResource = resourceManager.GetResource(Game.Domain, "shaders/text.vert.spv")!;
                var fsResource = resourceManager.GetResource(Game.Domain, "shaders/text.frag.spv")!;
                VertexShader vs = ctx.CreateVertexShader(vsResource)
                    .WithUniform<SimpleUniform>(out var uniform);
                FragmentShader fs = ctx.CreateFragmentShader(fsResource)
                    .WithSampler(out var textureHandle);
                RenderPipeline<TextVertex> pipeline = ctx.CreatePipeline<TextVertex>(
                    vs, fs,
                    renderStage, Topology.Triangles
                );

                // Create spritesheet stuff
                TextureSampler sampler = ctx.CreateTextureSampler(
                    TextureFiltering.Nearest, TextureFiltering.Nearest
                );
                TextureBinding textureBinding = ctx.CreateTextureBinding(
                    textureHandle,
                    sampler,
                    GameWindow.Resources!.FontTexture
                );

                // Projection uniform
                using var projUniformNativeBuffer = new NativeBuffer<SimpleUniform>
                {
                    new SimpleUniform()
                    {
                        Matrix = Matrix4x4.CreateOrthographic(1280, 720, -1, 1)
                    }
                };
                UniformBuffer<SimpleUniform> projUniformBuffer = ctx.CreateUniformBuffer(uniform, projUniformNativeBuffer);

                return new TextLayerData<TextVertex>(pipeline, textureBinding, projUniformBuffer);
            },
            data => data.Pipeline,
            (_, _) => throw new NotSupportedException(),
            (data, cmd) =>
            {
                cmd.Using(data.Pipeline, data.TextureBinding);
                cmd.Using(data.Pipeline, data.ProjUniformBuffer, 0);
            });

        
        public static readonly RenderLayer<UIVertex> Ui = RenderLayer<UIVertex>.Create(
            UIVertex.CreateTransformer,
            ctx => GameWindow.Resources!.UiRenderStage,
            (ctx, resourceManager, renderStage) =>
            {
                var vsResource = resourceManager.GetResource(Game.Domain, "shaders/text.vert.spv")!;
                var fsResource = resourceManager.GetResource(Game.Domain, "shaders/text.frag.spv")!;
                VertexShader vs = ctx.CreateVertexShader(vsResource)
                    .WithUniform<SimpleUniform>(out var uniform);
                FragmentShader fs = ctx.CreateFragmentShader(fsResource)
                    .WithSampler(out var textureHandle);
                RenderPipeline<UIVertex> pipeline = ctx.CreatePipeline<UIVertex>(
                    vs, fs,
                    renderStage, Topology.Triangles
                );

                // Create spritesheet stuff
                TextureSampler sampler = ctx.CreateTextureSampler(
                    TextureFiltering.Nearest, TextureFiltering.Nearest
                );
                TextureBinding textureBinding = ctx.CreateTextureBinding(
                    textureHandle,
                    sampler,
                    GameWindow.Resources!.UiTexture
                );

                // Projection uniform
                using var projUniformNativeBuffer = new NativeBuffer<SimpleUniform>
                {
                    new SimpleUniform()
                    {
                        Matrix = Matrix4x4.CreateOrthographic(1280, 720, -1, 1)
                    }
                };
                UniformBuffer<SimpleUniform> projUniformBuffer = ctx.CreateUniformBuffer(uniform, projUniformNativeBuffer);

                return new TextLayerData<UIVertex>(pipeline, textureBinding, projUniformBuffer);
            },
            data => data.Pipeline,
            (_, _) => throw new NotSupportedException(),
            (data, cmd) =>
            {
                cmd.Using(data.Pipeline, data.TextureBinding);
                cmd.Using(data.Pipeline, data.ProjUniformBuffer, 0);
            });

        private sealed class TextLayerData<TVertex> where TVertex : unmanaged
        {
            public readonly RenderPipeline<TVertex> Pipeline;
            public readonly TextureBinding TextureBinding;
            public readonly UniformBuffer<SimpleUniform> ProjUniformBuffer;

            public TextLayerData(
                RenderPipeline<TVertex> pipeline,
                TextureBinding textureBinding,
                UniformBuffer<SimpleUniform> projUniformBuffer
            )
            {
                Pipeline = pipeline;
                TextureBinding = textureBinding;
                ProjUniformBuffer = projUniformBuffer;
            }
        }
    }
}