using System;
using DigBuild.Engine.Networking;
using DigBuild.Registries;

namespace DigBuild.Client
{
    public sealed class GameClient : IDisposable
    {
        public static GameClient? Instance { get; internal set; }
        
        public GameState? State { get; private set; }

        public GameClient()
        {
            Instance = this;
        }

        public void Dispose()
        {
            State?.Dispose();
        }

        public void Connect(string hostname, int port)
        {
            if (State != null)
                throw new Exception("Already in game.");

            var networkManager = new ClientNetworkManager(hostname, port, GameRegistries.NetworkPackets);

            State = new GameState(networkManager);

            networkManager.Connection.StartHandlingPackets();
        }
    }
}