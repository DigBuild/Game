using System;
using System.Threading.Tasks;
using DigBuild.Client.Worlds;
using DigBuild.Engine.Entities;
using DigBuild.Engine.Impl.Worlds;
using DigBuild.Engine.Items;
using DigBuild.Engine.Math;
using DigBuild.Engine.Networking;
using DigBuild.Engine.Physics;
using DigBuild.Networking;
using DigBuild.Players;
using DigBuild.Registries;
using DigBuild.Worlds;

namespace DigBuild.Client
{
    public sealed class GameState : IDisposable
    {
        private readonly ClientNetworkManager _networkManager;
        private readonly TaskCompletionSource _ready = new();
        private readonly TaskCompletionSource _running = new();

        private readonly TickSource _tickSource = new();

        private readonly GameInput _input = new();
        private PlayerController _player = null!;
        private WorldRayCastContext _rayCastContext = null!;
        private GameWindow _window = null!;
        
        public Task Ready => _ready.Task;
        public Task Running => _running.Task;

        public ClientWorld World { get; }
        public EntityInstance Player => _player.Entity;
        public IConnection Connection => _networkManager.Connection;
        
        public GameState(ClientNetworkManager networkManager)
        {
            _networkManager = networkManager;
            World = new ClientWorld(_tickSource);
        }
        
        public void Dispose()
        {
            World.Dispose();
            _tickSource.Stop();
            _networkManager.Dispose();
        }
        
        private void Tick()
        {
            _input.Update();

            Connection.SendAsync(new PlayerMoveRotatePacket()
            {
                Position = _player.PhysicalEntity.Position,
                Pitch = _player.PhysicalEntity.Pitch,
                Yaw = _player.PhysicalEntity.Yaw
            });
            
            _player.UpdateMovement(_input);
            _player.UpdateHotbar(_input);
            
            var hit = Raycast.Cast(_rayCastContext, _player.GetCamera(0).Ray);
            _player.UpdateInteraction(_input, hit);
        }

        public void SetReady(WorldReadyPacket packet)
        {
            Console.WriteLine("Wooo! We're ready \\o/");

            _player = new PlayerController(World.AddPlayer(packet.PlayerPosition));
            // _player.Inventory.Hotbar[0].Item = new ItemInstance(GameItems.Stone, 64);
            // _player.Inventory.Hotbar[1].Item = new ItemInstance(GameItems.Dirt, 64);
            // _player.Inventory.Hotbar[2].Item = new ItemInstance(GameItems.Crafter, 64);
            // _player.Inventory.Hotbar[3].Item = new ItemInstance(GameItems.Glowy, 64);
            // _player.Inventory.Hotbar[4].Item = new ItemInstance(GameItems.Log, 64);
            // _player.Inventory.Hotbar[5].Item = new ItemInstance(GameItems.Leaves, 64);
            // _player.Inventory.Hotbar[6].Item = new ItemInstance(GameItems.LogSmall, 64);

            var log = GameRegistries.Items.GetOrNull(Game.Domain, "log")!;

            foreach (var (id, pos) in packet.OtherPlayerPositions)
            {
                World.AddEntity(GameEntities.Item, id)
                    .WithPosition(pos)
                    .WithItem(new ItemInstance(log, 1));
            }
            
            _rayCastContext = new WorldRayCastContext(World);
            _window = new GameWindow(_tickSource, _player, _input, _rayCastContext);
            
            World.ChunkManager.ChunkChanged += chunk => _window.OnChunkChanged(chunk);
            World.ChunkManager.ChunkUnloaded += chunk => _window.OnChunkUnloaded(chunk);
            World.EntityAdded += entity => _window.OnEntityAdded(entity);
            World.EntityRemoved += guid => _window.OnEntityRemoved(guid);
            
            World.BlockChanged += pos =>
            {
                ChunkBlockLight.Update(World, pos);
                foreach (var direction in Directions.All)
                    ChunkBlockLight.Update(World, pos.Offset(direction));
            };

            _tickSource.Tick += Tick;
            
            _ready.SetResult();
        }

        public void Run()
        {
            _window.OpenWaitClosed().Wait();
            _running.SetResult();
        }
    }
}