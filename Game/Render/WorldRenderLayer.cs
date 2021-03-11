using System;
using System.Numerics;
using DigBuild.Engine.Render;
using DigBuild.Platform.Render;
using DigBuild.Platform.Util;
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
                    .WithUniform<SimpleUniform>(out var uniform)
                    .WithUniform<SimpleUniform>(out var uniform2);
                FragmentShader fs = ctx.CreateFragmentShader(fsResource)
                    .WithSampler(out var textureHandle);
                RenderPipeline<SimpleVertex> pipeline = ctx.CreatePipeline<SimpleVertex>(
                    vs, fs,
                    renderStage, Topology.Triangles,
                    depthTest: new DepthTest(true, CompareOperation.LessOrEqual, true)
                );

                // Create spritesheet stuff
                TextureSampler sampler = ctx.CreateTextureSampler(
                    TextureFiltering.Nearest, TextureFiltering.Nearest
                );
                TextureBinding blockTextureBinding = ctx.CreateTextureBinding(
                    textureHandle,
                    sampler,
                    GameWindow.Resources!.BlockTexture
                );

                // Projection uniform
                using var projUniformNativeBuffer = new NativeBuffer<SimpleUniform>
                {
                    new SimpleUniform()
                    {
                        Matrix = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 2, 1280 / 720f, 0.001f, 10000f)
                                 * Matrix4x4.CreateRotationZ(MathF.PI)
                    }
                };
                UniformBuffer<SimpleUniform> projUniformBuffer = ctx.CreateUniformBuffer(uniform2, projUniformNativeBuffer);

                return new SimpleLayerData<SimpleVertex, SimpleUniform>(
                    pipeline, blockTextureBinding, projUniformBuffer, uniform,
                    mat => new SimpleUniform(){Matrix = mat}
                );
            },
            data => data.Pipeline,
            (data, pool) => data.CreateUniforms(pool),
            (data, cmd) =>
            {
                cmd.Using(data.Pipeline, data.TextureBinding);
                cmd.Using(data.Pipeline, data.ProjUniformBuffer, 0);
            });

        private sealed class SimpleLayerData<TVertex, TUniform>
            where TVertex : unmanaged
            where TUniform : unmanaged, IUniform<TUniform>
        {
            public readonly RenderPipeline<TVertex> Pipeline;
            public readonly TextureBinding TextureBinding;
            public readonly UniformBuffer<SimpleUniform> ProjUniformBuffer;
            private readonly UniformHandle<TUniform> _uniformHandle;
            private readonly Func<Matrix4x4, TUniform> _uniformFactory;

            public SimpleLayerData(RenderPipeline<TVertex> pipeline,
                TextureBinding textureBinding,
                UniformBuffer<SimpleUniform> projUniformBuffer,
                UniformHandle<TUniform> uniformHandle,
                Func<Matrix4x4, TUniform> uniformFactory)
            {
                Pipeline = pipeline;
                TextureBinding = textureBinding;
                ProjUniformBuffer = projUniformBuffer;
                _uniformHandle = uniformHandle;
                _uniformFactory = uniformFactory;
            }

            public IRenderLayerUniforms CreateUniforms(NativeBufferPool pool)
            {
                return new Uniforms(_uniformHandle, Pipeline, _uniformFactory, pool);
            }

            private sealed class Uniforms : IRenderLayerUniforms
            {
                private readonly UniformHandle<TUniform> _uniformHandle;
                private UniformBuffer<TUniform>? _uniformBuffer;
                private readonly Func<Matrix4x4, TUniform> _uniformFactory;

                private readonly RenderPipeline<TVertex> _pipeline;
                private readonly PooledNativeBuffer<TUniform> _nativeBuffer;

                public Uniforms(
                    UniformHandle<TUniform> uniformHandle,
                    RenderPipeline<TVertex> pipeline,
                    Func<Matrix4x4, TUniform> uniformFactory, NativeBufferPool pool)
                {
                    _uniformHandle = uniformHandle;
                    _pipeline = pipeline;
                    _uniformFactory = uniformFactory;
                    _nativeBuffer = pool.Request<TUniform>();
                }

                public void Setup(RenderContext context, CommandBufferRecorder cmd)
                {
                }

                public void PushAndUseTransform(RenderContext context, CommandBufferRecorder cmd, Matrix4x4 transform)
                {
                    _uniformBuffer ??= context.CreateUniformBuffer(_uniformHandle);

                    var index = _nativeBuffer.Count;
                    _nativeBuffer.Add(_uniformFactory(transform));
                    cmd.Using(_pipeline, _uniformBuffer, index);
                }

                public void Finalize(RenderContext context, CommandBufferRecorder cmd)
                {
                    _uniformBuffer!.Write(_nativeBuffer);
                }

                public void Clear()
                {
                    _nativeBuffer.Clear();
                }

                public void Dispose()
                {
                    _nativeBuffer.Dispose();
                }
            }
        }
    }

    public interface ISimpleUniform : IUniform<SimpleUniform>
    {
        public Matrix4x4 Matrix { get; set; }
    }
}