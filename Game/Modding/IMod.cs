using DigBuild.Engine.Events;

namespace DigBuild.Modding
{
    /// <summary>
    /// A mod. Must be implemented by all mod classes.
    /// </summary>
    public interface IMod
    {
        /// <summary>
        /// The resource domain of the mod.
        /// </summary>
        string Domain { get; }

        /// <summary>
        /// Sets up the mod.
        /// </summary>
        /// <param name="bus">The global event bus</param>
        void Setup(EventBus bus);
    }
}