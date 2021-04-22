using DigBuild.Engine.Networking;
using DigBuild.Engine.Registries;
using DigBuild.Networking;

namespace DigBuild.Registries
{
    public static class GamePackets
    {
        public static void Register(ExtendedTypeRegistryBuilder<IPacket, IPacketType> registry)
        {
            registry.Register(Game.Domain, "chunk_description", ChunkDescriptionPacket.Serdes);
            registry.Register(Game.Domain, "world_ready", WorldReadyPacket.Serdes);
            registry.Register(Game.Domain, "entity_move_rotate", EntityMoveRotatePacket.Serdes);
            registry.Register(Game.Domain, "player_move_rotate", PlayerMoveRotatePacket.Serdes);
            registry.Register(Game.Domain, "player_join", PlayerJoinPacket.Serdes);
        }
    }
}