using System;
using System.Linq;
using System.Numerics;
using DigBuild.Engine.Render;
using DigBuild.Platform.Render;
using DigBuild.Platform.Resource;
using DigBuild.Engine.BuiltIn.GeneratedUniforms;
using DigBuild.Render.GeneratedUniforms;

namespace DigBuild.Render
{
    public sealed class SimpleRenderLayer<TVertex> : IRenderLayer<TVertex>
        where TVertex : unmanaged
    {
        public delegate IVertexConsumer<TVertex> VertexTransformerProviderDelegate(IVertexConsumer<TVertex> consumer, Matrix4x4 transform, bool transformNormal);

        private readonly VertexTransformerProviderDelegate _transformerProvider;
        private readonly ResourceName _vertexShader;
        private readonly ResourceName _fragmentShader;
        private readonly RenderTexture _texture;
        private readonly bool _depthTest;
        private readonly bool _writeDepth;
        private readonly BlendOptions? _blend;

        private RenderResources _resources = null!;

        public SimpleRenderLayer(
            VertexTransformerProviderDelegate transformerProvider,
            ResourceName vertexShader,
            ResourceName fragmentShader,
            RenderTexture texture,
            bool depthTest = true,
            bool writeDepth = true,
            BlendOptions? blend = null
        )
        {
            _transformerProvider = transformerProvider;
            _vertexShader = vertexShader;
            _fragmentShader = fragmentShader;
            _texture = texture;
            _depthTest = depthTest;
            _writeDepth = writeDepth;
            _blend = blend;
        }

        public IVertexConsumer<TVertex> CreateTransformer(IVertexConsumer<TVertex> consumer, Matrix4x4 transform, bool transformNormal)
        {
            return _transformerProvider(consumer, transform, transformNormal);
        }

        public IVertexConsumer<TVertex> CreateLightingTransformer(IVertexConsumer<TVertex> consumer, Func<Vector3, Vector3, float> lightValueProvider)
        {
            return consumer; // TODO: Implement lighting calculations
        }

        public void InitResources(RenderContext context, ResourceManager resourceManager, RenderStage renderStage)
        {
            _resources = new RenderResources(context, resourceManager, renderStage, _vertexShader, _fragmentShader, _depthTest, _writeDepth, _blend);
        }

        public void SetupCommand(CommandBufferRecorder cmd, IReadOnlyUniformBufferSet uniforms, IReadOnlyTextureSet textures)
        {
            _resources.UniformBinding.Update(uniforms.Get(RenderUniforms.ModelViewTransform));
            _resources.WorldTimeUniformBinding.Update(uniforms.Get(RenderUniforms.WorldTime));
            _resources.TextureBinding.Update(textures.DefaultSampler, textures.Get(_texture));
            cmd.Using(_resources.Pipeline, _resources.TextureBinding);
        }

        public void Draw(CommandBufferRecorder cmd, IReadOnlyUniformBufferSet uniforms, VertexBuffer<TVertex> vertexBuffer)
        {
            cmd.Using(_resources.Pipeline, _resources.UniformBinding, uniforms.GetIndex(RenderUniforms.ModelViewTransform));
            cmd.Using(_resources.Pipeline, _resources.WorldTimeUniformBinding, uniforms.GetIndex(RenderUniforms.WorldTime));
            cmd.Draw(_resources.Pipeline, vertexBuffer);
        }

        private sealed class RenderResources
        {
            public readonly RenderPipeline<TVertex> Pipeline;
            public readonly UniformBinding<SimpleTransform> UniformBinding;
            public readonly UniformBinding<WorldTimeUniform> WorldTimeUniformBinding;
            public readonly TextureBinding TextureBinding;
            
            public RenderResources(
                RenderContext context,
                ResourceManager resourceManager,
                RenderStage renderStage,
                ResourceName vertexShader,
                ResourceName fragmentShader,
                bool depthTest,
                bool writeDepth,
                BlendOptions? blend
            )
            {
                var vsResource = resourceManager.Get<Shader>(vertexShader)!;
                var fsResource = resourceManager.Get<Shader>(fragmentShader)!;

                VertexShader vs = context.CreateVertexShader(vsResource.Resource)
                    .WithUniform<SimpleTransform>(out var vertexUniform);
                FragmentShader fs = context.CreateFragmentShader(fsResource.Resource)
                    .WithUniform<WorldTimeUniform>(out var worldTime)
                    .WithSampler(out var textureSampler);
                
                UniformBinding = context.CreateUniformBinding(vertexUniform);
                WorldTimeUniformBinding = context.CreateUniformBinding(worldTime);
                TextureBinding = context.CreateTextureBinding(textureSampler);

                var pipelineBuilder = context.CreatePipeline<TVertex>(
                    vs, fs, renderStage, Topology.Triangles
                );
                if (depthTest)
                    pipelineBuilder = pipelineBuilder.WithDepthTest(CompareOperation.LessOrEqual, writeDepth);
                if (blend != null)
                {
                    foreach (var attachment in renderStage.Format.Attachments.OfType<FramebufferColorAttachment>())
                    {
                        pipelineBuilder = pipelineBuilder.WithBlending(
                            attachment,
                            blend.From,
                            blend.To,
                            blend.Operation
                        );
                    }
                }
                Pipeline = pipelineBuilder;
            }
        }
    }

    public sealed class BlendOptions
    {
        public BlendFactor From { get; init; }
        public BlendFactor To { get; init; }
        public BlendOperation Operation { get; init; }
    }
}