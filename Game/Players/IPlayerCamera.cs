using DigBuild.Engine.Physics;
using DigBuild.Engine.Render;

namespace DigBuild.Players
{
    /// <summary>
    /// A player-specific camera capable of casting rays and performing underwater checks.
    /// </summary>
    public interface IPlayerCamera : ICamera
    {
        /// <summary>
        /// The ray cast by this camera.
        /// </summary>
        RayCaster.Ray Ray { get; }

        /// <summary>
        /// Whether the camera is underwater or not.
        /// </summary>
        bool IsUnderwater { get; }
    }
}