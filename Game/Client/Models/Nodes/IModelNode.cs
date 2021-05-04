using System.Collections.Generic;
using DigBuild.Engine.Render;

namespace DigBuild.Client.Models.Nodes
{
    public interface IModelNode
    {
        IEnumerable<IModelGeometry> GetGeometries(JsonModelData data);
    }

    public interface IModelGeometry
    {
        void Pipe(IGeometryConsumer consumer);
    }

    public interface IGeometryConsumer
    {
        IVertexConsumer<TVertex> Get<TVertex>(RenderLayer<TVertex> layer) where TVertex : unmanaged;
    }
}