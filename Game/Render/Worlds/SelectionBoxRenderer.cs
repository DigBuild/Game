using System.Numerics;
using DigBuild.Engine.BuiltIn.GeneratedUniforms;
using DigBuild.Engine.Events;
using DigBuild.Engine.Math;
using DigBuild.Engine.Physics;
using DigBuild.Engine.Render;
using DigBuild.Engine.Render.Worlds;
using DigBuild.Engine.Worlds;
using DigBuild.Events;
using DigBuild.Platform.Render;
using DigBuild.Platform.Resource;
using DigBuild.Platform.Util;
using DigBuild.Players;
using DigBuild.Worlds;

namespace DigBuild.Render.Worlds
{
    public sealed class SelectionBoxRenderer
    {
        private readonly IGridAlignedRayCastingContext<WorldRayCastContext.Hit> _rayCastingContext;
        private readonly IReadOnlyWorld _world;
        private readonly EventBus _eventBus;

        private WorldRayCastContext.Hit? _hit;
        
        private readonly NativeBuffer<Vertex3> _vertexNativeBuffer = new();
        private readonly NativeBuffer<SimpleTransform> _uniformNativeBuffer = new();
        
        private RenderPipeline<Vertex3> _pipeline = null!;
        private VertexBuffer<Vertex3> _vertexBuffer = null!;
        private VertexBufferWriter<Vertex3> _vertexBufferWriter = null!;
        private UniformBuffer<SimpleTransform> _uniformBuffer = null!;
        private UniformBinding<SimpleTransform> _uniformBinding = null!;

        public SelectionBoxRenderer(
            IGridAlignedRayCastingContext<WorldRayCastContext.Hit> rayCastingContext,
            IReadOnlyWorld world,
            EventBus eventBus
        )
        {
            _rayCastingContext = rayCastingContext;
            _world = world;
            _eventBus = eventBus;

            _uniformNativeBuffer.Add(default(SimpleTransform));
        }

        public void Dispose()
        {
            _vertexNativeBuffer.Dispose();
            _uniformNativeBuffer.Dispose();
        }

        public void Setup(RenderContext context, ResourceManager resourceManager, RenderStage renderStage)
        {
            var vsResource = resourceManager.Get<Shader>(DigBuildGame.Domain, "world/special/outline.vert")!;
            var fsResource = resourceManager.Get<Shader>(DigBuildGame.Domain, "world/special/outline.frag")!;
            
            VertexShader vs = context.CreateVertexShader(vsResource.Resource)
                .WithUniform<SimpleTransform>(out var uniform);
            FragmentShader fs = context.CreateFragmentShader(fsResource.Resource);
            _pipeline = context.CreatePipeline<Vertex3>(vs, fs, renderStage, Topology.Lines)
                .WithDepthTest(CompareOperation.LessOrEqual, false);

            _vertexBuffer = context.CreateVertexBuffer(out _vertexBufferWriter);

            _uniformBuffer = context.CreateUniformBuffer(_uniformNativeBuffer);
            _uniformBinding = context.CreateUniformBinding(uniform, _uniformBuffer);
        }
        
        public void Update(RenderContext context, WorldView worldView, float partialTick)
        {
            if (worldView.Camera is not IPlayerCamera playerCamera)
            {
                _hit = null;
                return;
            }

            _hit = RayCaster.TryCast(_rayCastingContext, playerCamera.Ray, out var h) ? h : null;
            
            _vertexNativeBuffer.Clear();

            var vertexConsumer = new NativeBufferVertexConsumer<Vertex3>(_vertexNativeBuffer);
            var evt = _eventBus.Post(new BlockHighlightEvent(vertexConsumer, _world, _hit));
            if (!evt.Handled && _hit != null)
                GenerateBoundingBoxGeometry(vertexConsumer, _hit.Bounds + _hit.Position);

            if (_vertexNativeBuffer.Count == 0)
                return;

            _vertexBufferWriter.Write(_vertexNativeBuffer);
                
            _uniformNativeBuffer[0].ModelView = worldView.Camera.Transform;
            _uniformNativeBuffer[0].Projection = worldView.Projection;
            _uniformBuffer.Write(_uniformNativeBuffer);
        }
        
        public void Record(RenderContext context, CommandBufferRecorder cmd)
        {
            if (_vertexNativeBuffer.Count == 0)
                return;

            cmd.Using(_pipeline, _uniformBinding, 0); 
            cmd.Draw(_pipeline, _vertexBuffer); 
        }

        public static void GenerateBoundingBoxGeometry(IVertexConsumer<Vertex3> buffer, AABB aabb)
        {
            var offset = new Vector3(0.005f);
            var min = aabb.Min - offset;
            var max = aabb.Max + offset;
            
            var vNNN = new Vector3(min.X, min.Y, min.Z);
            var vNNP = new Vector3(min.X, min.Y, max.Z);
            var vNPN = new Vector3(min.X, max.Y, min.Z);
            var vNPP = new Vector3(min.X, max.Y, max.Z);
            var vPNN = new Vector3(max.X, min.Y, min.Z);
            var vPNP = new Vector3(max.X, min.Y, max.Z);
            var vPPN = new Vector3(max.X, max.Y, min.Z);
            var vPPP = new Vector3(max.X, max.Y, max.Z);

            buffer.Accept(
                new Vertex3(vNNN), new Vertex3(vNNP),
                new Vertex3(vNNP), new Vertex3(vPNP),
                new Vertex3(vPNP), new Vertex3(vPNN),
                new Vertex3(vPNN), new Vertex3(vNNN),
                new Vertex3(vNNN), new Vertex3(vNPN),
                new Vertex3(vNNP), new Vertex3(vNPP),
                new Vertex3(vPNP), new Vertex3(vPPP),
                new Vertex3(vPNN), new Vertex3(vPPN),
                new Vertex3(vNPN), new Vertex3(vNPP),
                new Vertex3(vNPP), new Vertex3(vPPP),
                new Vertex3(vPPP), new Vertex3(vPPN),
                new Vertex3(vPPN), new Vertex3(vNPN)
            );
        }
    }
}