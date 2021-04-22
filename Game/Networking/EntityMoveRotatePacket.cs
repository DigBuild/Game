using System;
using System.Numerics;
using DigBuild.Client;
using DigBuild.Engine.Impl.Worlds;
using DigBuild.Engine.Networking;
using DigBuild.Engine.Serialization;
using DigBuild.Registries;

namespace DigBuild.Networking
{
    public sealed class EntityMoveRotatePacket : IPacket
    {
        public Guid Id { get; set; }
        public Vector3 Position { get; set; }
        public float Pitch { get; set; }
        public float Yaw { get; set; }

        public void Handle(IConnection connection)
        {
            var entity = GameClient.Instance?.State?.World.GetEntity(Id);
            if (entity == GameClient.Instance?.State?.Player)
                return;
            var physicalEntity = entity?.Get(EntityCapabilities.PhysicalEntity);
            if (physicalEntity == null)
                return;

            physicalEntity.Position = Position;
            physicalEntity.Pitch = Pitch;
            physicalEntity.Yaw = Yaw;
        }

        public static ISerdes<EntityMoveRotatePacket> Serdes { get; } = new CompositeSerdes<EntityMoveRotatePacket>()
        {
            {1u, p => p.Id, UnmanagedSerdes<Guid>.NotNull},
            {2u, p => p.Position, UnmanagedSerdes<Vector3>.NotNull},
            {3u, p => p.Pitch, UnmanagedSerdes<float>.NotNull},
            {4u, p => p.Yaw, UnmanagedSerdes<float>.NotNull},
        };
    }
}