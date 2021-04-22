using System;
using DigBuild.Engine.Networking;
using DigBuild.Engine.Serialization;
using DigBuild.Server;

namespace DigBuild.Networking
{
    public class KillServerPacket : IPacket
    {
        public void Handle(IConnection connection)
        {
            Console.WriteLine("Oh no.");
            GameServer.Instance?.Stop();
        }
        
        public static ISerdes<KillServerPacket> Serdes { get; } = EmptySerdes<KillServerPacket>.Instance;
    }
}