using System;
using DigBuild.Engine.Math;
using DigBuild.Engine.Render;
using DigBuild.Engine.Render.Models;
using DigBuild.Render.Worlds;

namespace DigBuild.Render.Models
{
    public sealed class SimpleModel : IBlockModel, IItemModel
    {
        private readonly WorldVertex[] _vertices;

        public SimpleModel(WorldVertex[] vertices)
        {
            _vertices = vertices;
        }
        
        public void AddGeometry(IGeometryBuffer buffer, IReadOnlyModelData data, DirectionFlags faces)
        {
            var consumer = buffer.Get(WorldRenderLayers.Opaque);
            consumer.Accept(_vertices);
        }

        public bool HasDynamicGeometry => false;

        public void AddDynamicGeometry(IGeometryBuffer buffer, IReadOnlyModelData data, DirectionFlags visibleFaces, float partialTick)
        {
            throw new InvalidOperationException();
        }

        public void AddGeometry(IGeometryBuffer buffer, IReadOnlyModelData data, ItemModelTransform transform, float partialTick)
        {
            buffer.Transform = transform.GetMatrix() * buffer.Transform;

            var consumer = buffer.Get(WorldRenderLayers.Opaque);
            consumer.Accept(_vertices);
        }
    }
}