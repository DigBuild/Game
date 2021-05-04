using System.Collections.Generic;
using System.Numerics;
using System.Text.Json;
using DigBuild.Engine.Render;
using DigBuild.Platform.Resource;
using DigBuild.Render;
using DigBuild.Serialization;

namespace DigBuild.Client.Models.Nodes
{
    public sealed class TransformModelNode : IModelNode, IModelGeometry
    {
        public Vector3 Translation { get; set; } = Vector3.Zero;
        public Vector3 Scale { get; set; } = Vector3.One;
        // public RotationTansform Rotation { get; set; }

        public IEnumerable<IModelGeometry> GetGeometries(JsonModelData data)
        {
            yield return this;
        }

        public void Pipe(IGeometryConsumer consumer)
        {
            // var vertexConsumer = consumer.Get(Layer);
        }
        
        public static IModelNode Parse(JsonElement json, IModelNodeParseContext context)
        {
            return json.Get<TransformModelNode>();
        }
    }
}