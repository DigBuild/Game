using System;
using System.Collections.Generic;
using System.Numerics;
using DigBuild.Client;
using DigBuild.Engine.Networking;
using DigBuild.Engine.Serialization;

namespace DigBuild.Networking
{
    public class WorldReadyPacket : IPacket
    {
        public Vector3 PlayerPosition { get; set; }

        public Dictionary<Guid, Vector3> OtherPlayerPositions { get; set; } = null!;

        public void Handle(IConnection connection)
        {
            GameClient.Instance!.State!.SetReady(this);
            Console.WriteLine($"Joined world with {OtherPlayerPositions.Count} other players");
        }

        public static ISerdes<WorldReadyPacket> Serdes { get; } = new CompositeSerdes<WorldReadyPacket>()
        {
            {1u, p => p.PlayerPosition, UnmanagedSerdes<Vector3>.NotNull},
            {2u, p => p.OtherPlayerPositions, new DictionarySerdes<Guid, Vector3>(
                UnmanagedSerdes<Guid>.NotNull,
                UnmanagedSerdes<Vector3>.NotNull
            )}
        };
    }
}