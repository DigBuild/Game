using System.Numerics;
using DigBuild.Engine.Networking;
using DigBuild.Engine.Serialization;
using DigBuild.Registries;
using DigBuild.Server;

namespace DigBuild.Networking
{
    public class PlayerMoveRotatePacket : IPacket
    {
        public Vector3 Position { get; set; }
        public float Pitch { get; set; }
        public float Yaw { get; set; }

        public void Handle(IConnection connection)
        {
            var player = GameServer.Instance!.Players.FindByConnection(connection);
            var physicalEntity = player.Entity.Get(EntityCapabilities.PhysicalEntity)!;
            
            physicalEntity.Position = Position;
            physicalEntity.Pitch = Pitch;
            physicalEntity.Yaw = Yaw;

            GameServer.Instance.SendToAllAsync(new EntityMoveRotatePacket()
            {
                Id = player.Entity.Id,
                Position = Position,
                Pitch = Pitch,
                Yaw = Yaw
            });
        }

        public static ISerdes<PlayerMoveRotatePacket> Serdes { get; } = new CompositeSerdes<PlayerMoveRotatePacket>()
        {
            {1u, p => p.Position, UnmanagedSerdes<Vector3>.NotNull},
            {2u, p => p.Pitch, UnmanagedSerdes<float>.NotNull},
            {3u, p => p.Yaw, UnmanagedSerdes<float>.NotNull},
        };
    }
}