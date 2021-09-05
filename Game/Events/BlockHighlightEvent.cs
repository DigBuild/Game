using DigBuild.Engine.Events;
using DigBuild.Engine.Render;
using DigBuild.Engine.Worlds;
using DigBuild.Render;
using DigBuild.Worlds;

namespace DigBuild.Events
{
    /// <summary>
    /// Fired when a block's wireframe is being drawn so a custom one can be provided.
    /// </summary>
    public sealed class BlockHighlightEvent : IEvent
    {
        /// <summary>
        /// The vertex consumer.
        /// </summary>
        public IVertexConsumer<Vertex3> VertexConsumer { get; }
        /// <summary>
        /// The world.
        /// </summary>
        public IReadOnlyWorld World { get; }
        /// <summary>
        /// The raycast hit.
        /// </summary>
        public WorldRayCastContext.Hit? Hit { get; }

        /// <summary>
        /// Whether the event was handled or not.
        /// If true, the default wireframe won't render.
        /// </summary>
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