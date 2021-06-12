using System.Numerics;
using DigBuild.Engine.Physics;
using DigBuild.Engine.Render;

namespace DigBuild.Players
{
    public sealed class PlayerCamera : Camera, IPlayerCamera
    {
        public PlayerCamera(Vector3 position, float pitch, float yaw, float fieldOfView, bool underwater) :
            base(position, pitch, yaw, fieldOfView)
        {
            IsUnderwater = underwater;
        }

        public Raycast.Ray Ray => new(Position, Forward * 5f);

        public bool IsUnderwater { get; }
    }
}