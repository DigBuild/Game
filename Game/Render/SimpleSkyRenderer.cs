using System;
using System.Numerics;
using DigBuild.Engine.Render;
using DigBuild.Engine.Worlds;
using DigBuild.Render.GeneratedUniforms;
using DigBuild.Platform.Render;
using DigBuild.Platform.Resource;
using DigBuild.Platform.Util;
using DigBuild.Worlds;

namespace DigBuild.Render
{
    public class SimpleSkyRenderer : ISkyRenderer
    {
        private readonly NativeBuffer<SimpleSkyVertexUniform> _vertUniformNativeBuffer = new();
        private readonly NativeBuffer<SimpleSkyFragmentUniform> _fragUniformNativeBuffer = new();
        private readonly IReadOnlyWorld _world;

        private RenderPipeline<Vertex2> _pipeline = null!;
        private UniformBuffer<SimpleSkyVertexUniform> _vertUniformBuffer = null!;
        private UniformBuffer<SimpleSkyFragmentUniform> _fragUniformBuffer = null!;
        private VertexBuffer<Vertex2> _vertexBuffer = null!;

        public SimpleSkyRenderer(IReadOnlyWorld world)
        {
            _world = world;
            _vertUniformNativeBuffer.Add(default(SimpleSkyVertexUniform));
            _fragUniformNativeBuffer.Add(default(SimpleSkyFragmentUniform));
        }

        public void Dispose()
        {
            _vertUniformNativeBuffer.Dispose();
            _fragUniformNativeBuffer.Dispose();
        }

        public void Setup(RenderContext context, ResourceManager resourceManager, RenderStage renderStage)
        {
            var vsResource = resourceManager.Get<Shader>(DigBuildGame.Domain, "world/special/sky.vert")!;
            var fsResource = resourceManager.Get<Shader>(DigBuildGame.Domain, "world/special/sky.frag")!;
            
            VertexShader vs = context.CreateVertexShader(vsResource.Resource)
                .WithUniform<SimpleSkyVertexUniform>(out var vertUniform);
            FragmentShader fs = context.CreateFragmentShader(fsResource.Resource)
                .WithUniform<SimpleSkyFragmentUniform>(out var fragUniform);
            _pipeline = context.CreatePipeline<Vertex2>(vs, fs, renderStage, Topology.Triangles);
            
            _vertUniformBuffer = context.CreateUniformBuffer(vertUniform);
            _fragUniformBuffer = context.CreateUniformBuffer(fragUniform);
            _vertexBuffer = context.CreateVertexBuffer(
                // Tri 1
                new Vertex2(0, 0),
                new Vertex2(1, 0),
                new Vertex2(1, 1),
                // Tri 2
                new Vertex2(1, 1),
                new Vertex2(0, 1),
                new Vertex2(0, 0)
            );
        }

        public void Update(RenderContext context, ICamera camera, ViewFrustum viewFrustum, Matrix4x4 projection, float partialTick)
        {
            var mat = camera.Transform * projection;
            mat.Translation = Vector3.Zero;
            Matrix4x4.Invert(mat, out var matInv);

            var timeOfDay = (_world.AbsoluteTime % World.DayDuration) / (float) World.DayDuration;
            var timeFactor = MathF.Sin(timeOfDay * 2 * MathF.PI) * 0.5f + 0.5f;

            _vertUniformNativeBuffer[0].Matrix = matInv;
            _fragUniformNativeBuffer[0].TimeFactor = timeFactor;
            _vertUniformBuffer.Write(_vertUniformNativeBuffer);
            _fragUniformBuffer.Write(_fragUniformNativeBuffer);
        }

        public void Record(RenderContext context, CommandBufferRecorder cmd)
        {
            cmd.Using(_pipeline, _vertUniformBuffer, 0);
            cmd.Using(_pipeline, _fragUniformBuffer, 0);
            cmd.Draw(_pipeline, _vertexBuffer);
        }
    }
    
    public interface ISimpleSkyVertexUniform : IUniform<SimpleSkyVertexUniform>
    {
        Matrix4x4 Matrix { get; set; }
    }
    
    public interface ISimpleSkyFragmentUniform : IUniform<SimpleSkyFragmentUniform>
    {
        float TimeFactor { get; set; }
    }
}