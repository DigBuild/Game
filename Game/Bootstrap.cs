using DigBuild.Engine;
using DigBuild.Engine.Events;
using DigBuild.Modding;
using DigBuild.Registries;

namespace DigBuild
{
    /// <summary>
    /// Main bootstrap class. The entry point for the game.
    /// </summary>
    public static class Bootstrap
    {
        /// <summary>
        /// The entry point for the game.
        /// </summary>
        public static void Main()
        {
            var eventBus = new EventBus();
            DigBuildEngine.Initialize(eventBus);
            
            ModLoader.Instance.LoadMods(eventBus);
            GameRegistries.Initialize(eventBus);

            DigBuildGame.Instance = new DigBuildGame(eventBus);
            DigBuildGame.Instance.Start();
            DigBuildGame.Instance.Await();
            DigBuildGame.Instance.Dispose();
        }
    }
}