using DigBuild.Engine.Entities;
using DigBuild.Engine.Events;
using DigBuild.Engine.Render;

namespace DigBuild.Render.Worlds
{
    /// <summary>
    /// Fired when an entity is about to be drawn so uniforms can be updated.
    /// </summary>
    public sealed class PushEntityUniformsEvent : IEvent
    {
        /// <summary>
        /// The entity.
        /// </summary>
        public IReadOnlyEntityInstance Entity { get; }
        /// <summary>
        /// The uniforms.
        /// </summary>
        public IUniformBufferSetWriter Uniforms { get; }

        public PushEntityUniformsEvent(IReadOnlyEntityInstance entity, IUniformBufferSetWriter uniforms)
        {
            Entity = entity;
            Uniforms = uniforms;
        }
    }
}