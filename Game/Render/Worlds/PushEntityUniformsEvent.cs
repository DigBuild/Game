using DigBuild.Engine.Entities;
using DigBuild.Engine.Events;
using DigBuild.Engine.Render;

namespace DigBuild.Render.Worlds
{
    public sealed class PushEntityUniformsEvent : IEvent
    {
        public IReadOnlyEntityInstance Entity { get; }
        public IUniformBufferSetWriter Uniforms { get; }

        public PushEntityUniformsEvent(IReadOnlyEntityInstance entity, IUniformBufferSetWriter uniforms)
        {
            Entity = entity;
            Uniforms = uniforms;
        }
    }
}