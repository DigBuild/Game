using DigBuild.Engine.Physics;
using DigBuild.Engine.Render;

namespace DigBuild.Players
{
    public interface IPlayerCamera : ICamera
    {
        Raycast.Ray Ray { get; }   
    }
}