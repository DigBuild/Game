using System;
using System.Numerics;
using DigBuild.Render.GeneratedUniforms;
using DigBuildEngine.Render;
using DigBuildPlatformCS.Render;
using DigBuildPlatformCS.Util;

namespace DigBuild.Render
{
    public static class WorldRenderLayer
    {
        public static readonly WorldRenderLayer<SimpleVertex> Opaque = WorldRenderLayer<SimpleVertex>.Create(
            SimpleVertex.CreateTransformer,
            ctx => GameWindow.Resources!.MainRenderStage,
            (ctx, resourceManager, renderStage) =>
            {
                var vsResource = resourceManager.GetResource(Game.Domain, "shaders/test.vert.spv")!;
                var fsResource = resourceManager.GetResource(Game.Domain, "shaders/test.frag.spv")!;
                VertexShader vs = ctx.CreateVertexShader(vsResource)
                    .WithUniform<SimpleUniform>(out var uniform)
                    .WithUniform<SimpleUniform>(out var uniform2);
                FragmentShader fs = ctx.CreateFragmentShader(fsResource);
                RenderPipeline<SimpleVertex> pipeline = ctx.CreatePipeline<SimpleVertex>(
                    vs, fs,
                    renderStage, Topology.Triangles,
                    depthTest: new DepthTest(true, CompareOperation.LessOrEqual, true)
                );
                return new SimpleLayerData<SimpleVertex, SimpleUniform>(
                    pipeline, uniform, uniform2,
                    mat => new SimpleUniform(){Matrix = mat}
                );
            },
            data => data.Pipeline,
            (data, pool) => data.CreateUniforms(pool)
        );

        private sealed class SimpleLayerData<TVertex, TUniform>
            where TVertex : unmanaged
            where TUniform : unmanaged, IUniform<TUniform>
        {
            public readonly RenderPipeline<TVertex> Pipeline;
            private readonly UniformHandle<TUniform> _uniformHandle;
            private readonly UniformHandle<SimpleUniform> _projUniformHandle;
            private readonly Func<Matrix4x4, TUniform> _uniformFactory;

            public SimpleLayerData(
                RenderPipeline<TVertex> pipeline,
                UniformHandle<TUniform> uniformHandle,
                UniformHandle<SimpleUniform> projUniformHandle,
                Func<Matrix4x4, TUniform> uniformFactory
            )
            {
                Pipeline = pipeline;
                _uniformHandle = uniformHandle;
                _uniformFactory = uniformFactory;
                _projUniformHandle = projUniformHandle;
            }

            public IWorldRenderLayerUniforms CreateUniforms(NativeBufferPool pool)
            {
                return new Uniforms(_uniformHandle, _projUniformHandle, Pipeline, _uniformFactory, pool);
            }

            private sealed class Uniforms : IWorldRenderLayerUniforms
            {
                private readonly UniformHandle<TUniform> _uniformHandle;
                private readonly UniformHandle<SimpleUniform> _projUniformHandle;
                private UniformBuffer<TUniform>? _uniformBuffer;
                private UniformBuffer<SimpleUniform>? _projUniformBuffer;
                private readonly Func<Matrix4x4, TUniform> _uniformFactory;

                private readonly RenderPipeline<TVertex> _pipeline;
                private readonly PooledNativeBuffer<TUniform> _nativeBuffer;

                public Uniforms(UniformHandle<TUniform> uniformHandle, UniformHandle<SimpleUniform> projUniformHandle, RenderPipeline<TVertex> pipeline, Func<Matrix4x4, TUniform> uniformFactory, NativeBufferPool pool)
                {
                    _uniformHandle = uniformHandle;
                    _projUniformHandle = projUniformHandle;
                    _pipeline = pipeline;
                    _uniformFactory = uniformFactory;
                    _nativeBuffer = pool.Request<TUniform>();
                }

                public void Setup(RenderContext context, CommandBufferRecorder cmd)
                {
                    if (_projUniformBuffer == null)
                    {
                        using var buffer = new NativeBuffer<SimpleUniform>
                        {
                            new SimpleUniform()
                            {
                                Matrix = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 2, 1280 / 720f, 0.001f, 10000f)
                                         * Matrix4x4.CreateScale(1, -1, 1)
                            }
                        };
                        _projUniformBuffer = context.CreateUniformBuffer(_projUniformHandle, buffer);
                    }
                    cmd.Using(_pipeline, _projUniformBuffer, 0);
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