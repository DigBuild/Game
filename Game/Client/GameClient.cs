using System;
using DigBuild.Engine.Networking;
using DigBuild.Registries;

namespace DigBuild.Client
{
    public sealed class GameClient : IDisposable
    {
        public static GameClient? Instance { get; internal set; }

        private ClientNetworkManager? _networkManager;

        public Connection? Connection => _networkManager?.Connection;

        public GameClient()
        {
            Instance = this;
            GameRegistries.Initialize();
        }

        public void Dispose()
        {
            _networkManager?.Dispose();
        }

        public void Connect(string hostname, int port)
        {
            if (_networkManager != null)
                throw new Exception("Already connected.");

            _networkManager = new ClientNetworkManager(hostname, port, GameRegistries.NetworkPackets);
        }
    }
}