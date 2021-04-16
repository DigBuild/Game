using System;
using DigBuild.Engine.Networking;
using DigBuild.Engine.Serialization;

namespace DigBuild.Networking
{
    public class TestPacket : IPacket
    {
        public int Number { get; init; }

        public void Handle(Connection connection)
        {
            Console.WriteLine($"Received number: {Number}");
            if (new Random().NextDouble() > 0.6)
            {
                Console.WriteLine("Killing server!");
                connection.Send(new KillServerPacket());
            }
        }

        public static ISerdes<TestPacket> Serdes { get; } = new CompositeSerdes<TestPacket>(() => new TestPacket())
        {
            {1u, p => p.Number, UnmanagedSerdes<int>.NotNull}
        };
    }
}