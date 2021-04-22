using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DigBuild.Engine.Networking;
using DigBuild.Engine.Worldgen;
using DigBuild.Networking;
using DigBuild.Registries;
using DigBuild.Worlds;

namespace DigBuild.Server
{
    public sealed class GameServer : IDisposable
    {
        public static GameServer? Instance { get; internal set; }

        private readonly ServerNetworkManager _networkManager;
        private readonly TaskCompletionSource _running = new();

        private readonly TickSource _tickSource;
        private readonly World _world;
        
        public Task Running => _running.Task;

        public GameServer(int port)
        {
            Instance = this;

            _networkManager = new ServerNetworkManager(port, GameRegistries.NetworkPackets);
            _networkManager.ClientConnected += OnClientConnected;
            
            var features = new List<IWorldgenFeature>
            {
                // WorldgenFeatures.Terrain,
                // WorldgenFeatures.Water,
                // WorldgenFeatures.Lushness,
                // WorldgenFeatures.Trees
            };
            
            _tickSource = new TickSource();
            var generator = new WorldGenerator(_tickSource, features, 0);
            _world = new World(generator, _tickSource);
        }

        private void OnClientConnected(Connection connection)
        {
            Console.WriteLine($"Client connected {connection.GetHashCode()}");
            connection.Closed += () => Console.WriteLine($"Client disconnected {connection.GetHashCode()}");

            connection.Send(new TestPacket()
            {
                Number = connection.GetHashCode()
            });
        }

        public void Stop()
        {
            if (Running.IsCompleted)
                throw new Exception("Server is not running.");

            _tickSource.Stop();
            
            _networkManager.Close();
            _running.SetResult();
        }

        public void Dispose()
        {
            if (!Running.IsCompleted)
            {
                Console.WriteLine("Force stopping server.");
                Stop();
            }
            
            _world.Dispose();
            _networkManager.Dispose();
        }
    }
}