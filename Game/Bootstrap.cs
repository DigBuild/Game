using DigBuild.Engine;
using DigBuild.Engine.Events;
using DigBuild.Modding;
using DigBuild.Registries;

namespace DigBuild
{
    public static class Bootstrap
    {
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