using System.Linq;
using System.Numerics;
using DigBuild.Engine.Render;
using DigBuild.Engine.Render.New;
using DigBuild.Platform.Render;
using DigBuild.Platform.Resource;
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
        private readonly bool _depthTest;
        private readonly BlendOptions? _blend;

        private RenderResources _resources = null!;

        public SimpleRenderLayer(
            VertexTransformerProviderDelegate transformerProvider,
            ResourceName vertexShader,
            ResourceName fragmentShader,
            bool depthTest = true,
            BlendOptions? blend = null
        )
        {
            _transformerProvider = transformerProvider;
            _vertexShader = vertexShader;
            _fragmentShader = fragmentShader;
            _depthTest = depthTest;
            _blend = blend;
        }

        public IVertexConsumer<TVertex> CreateTransformer(IVertexConsumer<TVertex> consumer, Matrix4x4 transform, bool transformNormal)
        {
            return _transformerProvider(consumer, transform, transformNormal);
        }

        public void InitResources(RenderContext context, ResourceManager resourceManager, RenderStage renderStage)
        {
            _resources = new RenderResources(context, resourceManager, renderStage, _vertexShader, _fragmentShader, _depthTest, _blend);
        }

        public void SetupCommand(CommandBufferRecorder cmd, IReadOnlyUniformBufferSet uniforms, IReadOnlyTextureSet textures)
        {
            _resources.UniformBinding.Update(uniforms.Get(RenderUniforms.SimpleTransform));
            _resources.TextureBinding.Update(textures.DefaultSampler, textures.Get(RenderTextures.Main));
            cmd.Using(_resources.Pipeline, _resources.TextureBinding);
        }

        public void Draw(CommandBufferRecorder cmd, IReadOnlyUniformBufferSet uniforms, VertexBuffer<TVertex> vertexBuffer)
        {
            cmd.Using(_resources.Pipeline, _resources.UniformBinding, uniforms.GetIndex(RenderUniforms.SimpleTransform));
            cmd.Draw(_resources.Pipeline, vertexBuffer);
        }

        private sealed class RenderResources
        {
            public readonly RenderPipeline<TVertex> Pipeline;
            public readonly UniformBinding<SimpleTransform> UniformBinding;
            public readonly TextureBinding TextureBinding;
            
            public RenderResources(
                RenderContext context,
                ResourceManager resourceManager,
                RenderStage renderStage,
                ResourceName vertexShader,
                ResourceName fragmentShader,
                bool depthTest,
                BlendOptions? blend
            )
            {
                var vsResource = resourceManager.Get<Shader>(vertexShader)!;
                var fsResource = resourceManager.Get<Shader>(fragmentShader)!;

                VertexShader vs = context.CreateVertexShader(vsResource.Resource)
                    .WithUniform<SimpleTransform>(out var uniform);
                FragmentShader fs = context.CreateFragmentShader(fsResource.Resource)
                    .WithSampler(out var sampler);
                
                UniformBinding = context.CreateUniformBinding(uniform);
                TextureBinding = context.CreateTextureBinding(sampler);

                var pipelineBuilder = context.CreatePipeline<TVertex>(
                    vs,
                    fs,
                    renderStage, Topology.Triangles
                );
                if (depthTest)
                    pipelineBuilder = pipelineBuilder.WithDepthTest(CompareOperation.LessOrEqual, true);
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