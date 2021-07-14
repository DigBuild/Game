using DigBuild.Engine.Events;
using DigBuild.Engine.Render;
using DigBuild.Engine.Worlds;
using DigBuild.Render;
using DigBuild.Worlds;

namespace DigBuild.Events
{
    public sealed class BlockHighlightEvent : IEvent
    {
        public IVertexConsumer<Vertex3> VertexConsumer { get; }
        public IReadOnlyWorld World { get; }
        public WorldRayCastContext.Hit? Hit { get; }

        public bool Handled { get; set; }

        public BlockHighlightEvent(
            IVertexConsumer<Vertex3> vertexConsumer,
            IReadOnlyWorld world,
            WorldRayCastContext.Hit? hit
        )
        {
            VertexConsumer = vertexConsumer;
            World = world;
            Hit = hit;
        }
    }
}