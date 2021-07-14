using DigBuild.Engine.Events;
using DigBuild.Engine.Render;
using DigBuild.Render;

namespace DigBuild.Events
{
    public sealed class BlockHighlightEvent : IEvent
    {
        public IVertexConsumer<Vertex3> VertexConsumer { get; }

        public bool Handled { get; set; }

        public BlockHighlightEvent(IVertexConsumer<Vertex3> vertexConsumer)
        {
            VertexConsumer = vertexConsumer;
        }
    }
}