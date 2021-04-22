using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using DigBuild.Engine.Impl.Worlds;
using DigBuild.Engine.Math;
using DigBuild.Engine.Networking;
using DigBuild.Engine.Worldgen;
using DigBuild.Networking;
using DigBuild.Players;
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
        private readonly List<ServerPlayer> _players = new();

        private HashSet<ChunkPos> _chunksToSynchronize = new();
        private HashSet<ChunkPos> _chunksToSynchronize2 = new();
        
        public Task Running => _running.Task;

        public ServerConfig Config { get; }

        public IEnumerable<ServerPlayer> Players => _players;

        public GameServer(int port = 0)
        {
            Instance = this;

            Config = ServerConfig.Load("server/config.json");

            _networkManager = new ServerNetworkManager(port != 0 ? port : Config.Port, GameRegistries.NetworkPackets);
            _networkManager.ClientConnected += OnClientConnected;
            
            _tickSource = new TickSource();
            var generator = new WorldGenerator(_tickSource, Config.Worldgen.Features, 0);
            _world = new World(generator, _tickSource);
            
            _world.RegionManager.ChunkChanged += chunk =>
            {
                lock (_chunksToSynchronize)
                {
                    _chunksToSynchronize.Add(chunk.Position);
                }
            };
            _tickSource.Tick += () =>
            {
                lock (_chunksToSynchronize)
                {
                    (_chunksToSynchronize, _chunksToSynchronize2) = (_chunksToSynchronize2, _chunksToSynchronize);
                }
                
                var tasks = new List<Task>();
                foreach (var pos in _chunksToSynchronize2)
                {
                    var chunk = _world.GetChunk(pos, false);
                    if (chunk is Chunk c)
                        tasks.Add(_networkManager.SendToAllAsync(new ChunkDescriptionPacket { Chunk = c }));
                }
                Task.WaitAll(tasks.ToArray());
                
                _chunksToSynchronize2.Clear();
            };
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

        private void OnClientConnected(Connection connection)
        {
            Console.WriteLine($"Client connected {connection.GetHashCode()}");
            
            var player = _world.AddPlayer(new Vector3(0, 50, 0));
            var serverPlayer = new ServerPlayer(player.Entity, connection);
            
            connection.Closed += () =>
            {
                Console.WriteLine($"Client disconnected {connection.GetHashCode()}");
                lock (_players)
                {
                    _players.Remove(serverPlayer);
                }
            };

            Dictionary<Guid, Vector3> otherPlayerPositions;
            lock (_players)
            {
                otherPlayerPositions = _players.ToDictionary(
                    otherPlayer => otherPlayer.Entity.Id,
                    otherPlayer => otherPlayer.Entity.Get(EntityCapabilities.PhysicalEntity)!.Position
                );

                foreach (var otherPlayer in _players)
                {
                    otherPlayer.Connection.SendAsync(new PlayerJoinPacket()
                    {
                        Id = player.Entity.Id,
                        Position = player.PhysicalEntity.Position
                    });
                }

                _players.Add(serverPlayer);
            }

            connection.Send(new WorldReadyPacket()
            {
                PlayerPosition = player.PhysicalEntity.Position,
                OtherPlayerPositions = otherPlayerPositions
            });

            var tasks = new List<Task>();

            const ushort radius = 2;
            var chunkPos = new BlockPos(player.PhysicalEntity.Position).ChunkPos;
            for (var x = -radius; x <= radius; x++)
            for (var y = -3; y <= 0; y++)
            for (var z = -radius; z <= radius; z++)
            {
                var pos = new ChunkPos(chunkPos.X + x, chunkPos.Y + y, chunkPos.Z + z);
                var chunk = (Chunk) _world.GetChunk(pos)!;
                tasks.Add(connection.SendAsync(new ChunkDescriptionPacket { Chunk = chunk }));
            }

            Task.WaitAll(tasks.ToArray());
        }

        public void Start()
        {
            _tickSource.Start();
        }

        public void Stop()
        {
            if (Running.IsCompleted)
                throw new Exception("Server is not running.");

            _tickSource.Stop();
            
            _networkManager.Close();
            _running.SetResult();
        }

        
        public void SendToAll<T>(T packet) where T : IPacket
        {
            _networkManager.SendToAll(packet);
        }

        public Task SendToAllAsync<T>(T packet) where T : IPacket
        {
            return _networkManager.SendToAllAsync(packet);
        }
    }
}