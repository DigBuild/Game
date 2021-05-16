﻿using System.Collections.Generic;
using DigBuild.Engine.Render;

namespace DigBuild.Render.Models.Nodes
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
        IVertexConsumer<TVertex> Get<TVertex>(IRenderLayer<TVertex> layer) where TVertex : unmanaged;
    }
}