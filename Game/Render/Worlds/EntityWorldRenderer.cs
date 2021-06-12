using System.Collections.Generic;
using System.Numerics;
using DigBuild.Engine.BuiltIn;
using DigBuild.Engine.BuiltIn.GeneratedUniforms;
using DigBuild.Engine.Entities;
using DigBuild.Engine.Events;
using DigBuild.Engine.Render;
using DigBuild.Engine.Render.Models;
using DigBuild.Engine.Render.Worlds;
using DigBuild.Engine.Worlds;
using DigBuild.Platform.Render;
using DigBuild.Platform.Util;

namespace DigBuild.Render.Worlds
{
    public sealed class EntityWorldRenderer : IWorldRenderer
    {
        private readonly EventBus _eventBus;
        private readonly IReadOnlyDictionary<Entity, IEntityModel> _entityModels;

        private readonly GeometryBuffer _geometryBuffer;

        private readonly HashSet<IReadOnlyEntityInstance> _trackedEntities = new();
        private readonly HashSet<IReadOnlyEntityInstance> _addedEntities = new();
        private readonly HashSet<IReadOnlyEntityInstance> _removedEntities = new();

        private UniformBufferSet.Snapshot _uniforms = null!;

        public EntityWorldRenderer(EventBus eventBus, IReadOnlyDictionary<Entity, IEntityModel> entityModels, NativeBufferPool bufferPool)
        {
            _eventBus = eventBus;
            _entityModels = entityModels;

            _geometryBuffer = new GeometryBuffer(bufferPool);
            
            _eventBus.Subscribe<BuiltInEntityEvent.JoinedWorld>(OnEntityJoinedWorld);
            _eventBus.Subscribe<BuiltInEntityEvent.LeavingWorld>(OnEntityLeavingWorld);
        }

        public void Dispose()
        {
            _eventBus.Unsubscribe<BuiltInEntityEvent.JoinedWorld>(OnEntityJoinedWorld);
            _eventBus.Unsubscribe<BuiltInEntityEvent.LeavingWorld>(OnEntityLeavingWorld);
        }

        private void OnEntityJoinedWorld(BuiltInEntityEvent.JoinedWorld evt)
        {
            lock (_addedEntities)
                _addedEntities.Add(evt.Entity);
        }

        private void OnEntityLeavingWorld(BuiltInEntityEvent.LeavingWorld evt)
        {
            lock (_removedEntities)
                _removedEntities.Add(evt.Entity);
        }

        public void Update(RenderContext context, WorldView worldView, float partialTick)
        {
            lock (_removedEntities)
            {
                _trackedEntities.ExceptWith(_removedEntities);
                _removedEntities.Clear();
            }

            lock (_addedEntities)
            {
                _trackedEntities.UnionWith(_addedEntities);
                _addedEntities.Clear();
            }
            
            _geometryBuffer.Reset();
            foreach (var entity in _trackedEntities)
            {
                if (!_entityModels.TryGetValue(entity.Type, out var model))
                    continue;

                var modelData = entity.Get(ModelData.EntityAttribute);

                _geometryBuffer.Transform = Matrix4x4.Identity;
                _geometryBuffer.TransformNormal = true;
                model.AddGeometry(_geometryBuffer, modelData, partialTick);
            }
            _geometryBuffer.Upload(context);
        }

        public void BeforeDraw(RenderContext context, CommandBufferRecorder cmd, UniformBufferSet uniforms, WorldView worldView, float partialTick)
        {
            uniforms.Push(RenderUniforms.ModelViewTransform,
                new SimpleTransform
                {
                    ModelView = worldView.Camera.Transform,
                    Projection = worldView.Projection
                }
            );
            _uniforms = uniforms.CaptureSnapshot();
        }

        public void Draw(RenderContext context, CommandBufferRecorder cmd, IRenderLayer layer, IReadOnlyUniformBufferSet uniforms, WorldView worldView, float partialTick)
        {
            _geometryBuffer.Draw(cmd, layer, _uniforms);
        }

        public void AfterDraw(RenderContext context, CommandBufferRecorder cmd, WorldView worldView, float partialTick)
        {
        }
    }
}