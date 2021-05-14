using System;
using System.Numerics;
using DigBuild.Engine.Render;
using DigBuild.Platform.Render;
using DigBuild.Platform.Util;
using DigBuild.Render.GeneratedUniforms;

namespace DigBuild.Render
{
    public sealed class SimpleLayerData<TVertex, TUniform>
        where TVertex : unmanaged
        where TUniform : unmanaged, IUniform<TUniform>
    {
        public readonly RenderPipeline<TVertex> Pipeline;
        public readonly TextureBinding TextureBinding;
        private readonly UniformHandle<TUniform> _uniformHandle;
        private readonly Func<Matrix4x4, TUniform> _uniformFactory;

        public SimpleLayerData(RenderPipeline<TVertex> pipeline,
            TextureBinding textureBinding,
            UniformHandle<TUniform> uniformHandle,
            Func<Matrix4x4, TUniform> uniformFactory)
        {
            Pipeline = pipeline;
            TextureBinding = textureBinding;
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
            private UniformBinding<TUniform>? _uniformBinding;
            private readonly Func<Matrix4x4, TUniform> _uniformFactory;

            private readonly RenderPipeline<TVertex> _pipeline;
            private readonly PooledNativeBuffer<TUniform> _nativeBuffer;

            public Uniforms(
                UniformHandle<TUniform> uniformHandle,
                RenderPipeline<TVertex> pipeline,
                Func<Matrix4x4, TUniform> uniformFactory, NativeBufferPool pool
            )
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
                _uniformBuffer ??= context.CreateUniformBuffer<TUniform>();
                _uniformBinding ??= context.CreateUniformBinding(_uniformHandle, _uniformBuffer);

                var index = _nativeBuffer.Count;
                _nativeBuffer.Add(_uniformFactory(transform));
                cmd.Using(_pipeline, _uniformBinding, index);
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

    public interface ISimpleUniform : IUniform<SimpleUniform>
    {
        public Matrix4x4 Matrix { get; set; }
    }
}