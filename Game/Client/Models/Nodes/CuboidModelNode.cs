using System.Collections.Generic;
using System.Numerics;
using System.Text.Json;
using DigBuild.Engine.Render;
using DigBuild.Platform.Resource;
using DigBuild.Render;
using DigBuild.Serialization;

namespace DigBuild.Client.Models.Nodes
{
    public sealed class CuboidModelNode : IModelNode, IModelGeometry
    {
        public Vector3 From { get; set; }
        public Vector3 To { get; set; }
        public RenderLayer<SimpleVertex> Layer { get; set; }
        public CuboidTextures Textures { get; set; }

        public IEnumerable<IModelGeometry> GetGeometries()
        {
            yield return this;
        }

        public void Pipe(IGeometryConsumer consumer)
        {
            var vertexConsumer = consumer.Get(Layer);
        }
        
        public static IModelNode Parse(JsonElement json, IModelNodeParseContext context)
        {
            return json.Get<CuboidModelNode>();
        }

        public sealed class CuboidTextures
        {
            public ResourceName? NegX { get; set; }
            public ResourceName? PosX { get; set; }
            public ResourceName? NegY { get; set; }
            public ResourceName? PosY { get; set; }
            public ResourceName? NegZ { get; set; }
            public ResourceName? PosZ { get; set; }
        }
    }
}