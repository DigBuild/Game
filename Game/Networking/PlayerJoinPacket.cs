using System;
using System.Numerics;
using DigBuild.Client;
using DigBuild.Engine.Impl.Worlds;
using DigBuild.Engine.Items;
using DigBuild.Engine.Networking;
using DigBuild.Engine.Serialization;
using DigBuild.Registries;

namespace DigBuild.Networking
{
    public class PlayerJoinPacket : IPacket
    {
        public Guid Id { get; set; }
        public Vector3 Position { get; set; }

        public void Handle(IConnection connection)
        {
            var log = GameRegistries.Items.GetOrNull(Game.Domain, "log")!;
            GameClient.Instance!.State!.World.AddEntity(GameEntities.Item, Id)
                .WithPosition(Position)
                .WithItem(new ItemInstance(log, 1));
            Console.WriteLine($"Player spawned with ID {Id}");
        }
        
        public static ISerdes<PlayerJoinPacket> Serdes { get; } = new CompositeSerdes<PlayerJoinPacket>()
        {
            {1u, p => p.Id, UnmanagedSerdes<Guid>.NotNull},
            {2u, p => p.Position, UnmanagedSerdes<Vector3>.NotNull},
        };
    }
}