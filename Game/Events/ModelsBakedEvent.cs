using DigBuild.Engine.Events;
using DigBuild.Render;

namespace DigBuild.Events
{
    /// <summary>
    /// Fired when all models have been baked.
    /// At this point it is possible to safely make overrides.
    /// </summary>
    public class ModelsBakedEvent : IEvent
    {
        /// <summary>
        /// The model manager.
        /// </summary>
        public ModelManager ModelManager { get; }

        public ModelsBakedEvent(ModelManager modelManager)
        {
            ModelManager = modelManager;
        }
    }
}