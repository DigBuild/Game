using DigBuild.Engine.Physics;
using DigBuild.Engine.Render;

namespace DigBuild.Players
{
    public interface IPlayerCamera : ICamera
    {
        RayCaster.Ray Ray { get; }

        bool IsUnderwater { get; }
    }
}