using DigBuild.Engine.Events;

namespace DigBuild.Modding
{
    public interface IMod
    {
        void AttachEvents(EventBus bus);
    }
}