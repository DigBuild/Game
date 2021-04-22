using System.Collections.Generic;
using System.Linq;
using DigBuild.Engine.Entities;
using DigBuild.Engine.Networking;

namespace DigBuild.Server
{
    public sealed class ServerPlayer
    {
        public EntityInstance Entity { get; }
        public IConnection Connection { get; }

        public ServerPlayer(EntityInstance entity, IConnection connection)
        {
            Entity = entity;
            Connection = connection;
        }
    }

    public static class ServerPlayerExtensions
    {
        public static ServerPlayer FindByConnection(this IEnumerable<ServerPlayer> players, IConnection connection)
        {
            return players.First(p => p.Connection == connection);
        }
    }
}