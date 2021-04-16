using DigBuild.Engine.Networking;
using DigBuild.Engine.Registries;
using DigBuild.Networking;

namespace DigBuild.Registries
{
    public static class GamePackets
    {
        public static void Register(ExtendedTypeRegistryBuilder<IPacket, IPacketType> registry)
        {
            registry.Register(Game.Domain, "test_packet", TestPacket.Serdes);
            registry.Register(Game.Domain, "kill_server", KillServerPacket.Serdes);
        }
    }
}