using DigBuild.Engine.Events;
using DigBuild.Render;

namespace DigBuild.Events
{
    public class ModelsBakedEvent : IEvent
    {
        public ModelManager ModelManager { get; }

        public ModelsBakedEvent(ModelManager modelManager)
        {
            ModelManager = modelManager;
        }
    }
}