using DigBuild.Engine.Events;

namespace DigBuild.Modding
{
    public interface IMod
    {
        string Domain { get; }

        void Setup(EventBus bus);
    }
}