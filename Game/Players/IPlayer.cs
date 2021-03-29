using DigBuild.Behaviors;
using DigBuild.Engine.Entities;

namespace DigBuild.Players
{
    public interface IPlayer
    {
        EntityInstance Entity { get; }
        IPhysicalEntity PhysicalEntity { get; }
        PlayerInventory Inventory { get; }

        IPlayerCamera GetCamera(float partialTick);
    }
}