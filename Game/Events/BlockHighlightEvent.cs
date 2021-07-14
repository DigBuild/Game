using DigBuild.Engine.Events;
using DigBuild.Engine.Render;
using DigBuild.Render;
using DigBuild.Worlds;

namespace DigBuild.Events
{
    public sealed class BlockHighlightEvent : IEvent
    {
        public IVertexConsumer<Vertex3> VertexConsumer { get; }
        public WorldRayCastContext.Hit? Hit { get; }

        public bool Handled { get; set; }

        public BlockHighlightEvent(IVertexConsumer<Vertex3> vertexConsumer, WorldRayCastContext.Hit? hit)
        {
            VertexConsumer = vertexConsumer;
            Hit = hit;
        }
    }
}