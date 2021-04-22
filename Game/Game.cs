using System;
using System.Threading;
using System.Threading.Tasks;
using DigBuild.Client;
using DigBuild.Engine.Events;
using DigBuild.Modding;
using DigBuild.Recipes;
using DigBuild.Registries;
using DigBuild.Server;

namespace DigBuild
{
    public static class Game// : IDisposable
    {
        public const string Domain = "digbuild";
        public const int ViewRadius = 12;
        public static CraftingRecipeLookup RecipeLookup { get; private set; } = null!;

        // private readonly GameWindow _window;
        //
        // private readonly GameInput _input = new();
        //
        // private readonly PlayerController _player;
        // private readonly WorldRayCastContext _rayCastContext;
        //
        // public Game()
        // {
        //     GameRegistries.Initialize();
        //
        //     _tickSource = new TickSource();
        //     _tickSource.Tick += Tick;
        //     
        //     RecipeLookup = new CraftingRecipeLookup(GameRegistries.CraftingRecipes.Values);
        //
        //     var features = new List<IWorldgenFeature>
        //     {
        //         WorldgenFeatures.Terrain,
        //         WorldgenFeatures.Water,
        //         WorldgenFeatures.Lushness,
        //         WorldgenFeatures.Trees
        //     };
        //     var generator = new WorldGenerator(_tickSource, features, 0);
        //
        //     _world = new World(generator, _tickSource);
        //     _rayCastContext = new WorldRayCastContext(_world);
        //
        //     _player = new PlayerController(_world.AddPlayer(new Vector3(0, 50, 0)));
        //     _player.Inventory.Hotbar[0].Item = new ItemInstance(GameItems.Stone, 64);
        //     _player.Inventory.Hotbar[1].Item = new ItemInstance(GameItems.Dirt, 64);
        //     _player.Inventory.Hotbar[2].Item = new ItemInstance(GameItems.Crafter, 64);
        //     _player.Inventory.Hotbar[3].Item = new ItemInstance(GameItems.Glowy, 64);
        //     _player.Inventory.Hotbar[4].Item = new ItemInstance(GameItems.Log, 64);
        //     _player.Inventory.Hotbar[5].Item = new ItemInstance(GameItems.Leaves, 64);
        //     _player.Inventory.Hotbar[6].Item = new ItemInstance(GameItems.LogSmall, 64);
        //     
        //     _window = new GameWindow(_tickSource, _player, _input, _rayCastContext);
        //     
        //     _world.ChunkManager.ChunkChanged += chunk => _window.OnChunkChanged(chunk);
        //     _world.ChunkManager.ChunkUnloaded += chunk => _window.OnChunkUnloaded(chunk);
        //     _world.EntityAdded += entity => _window.OnEntityAdded(entity);
        //     _world.EntityRemoved += guid => _window.OnEntityRemoved(guid);
        //
        //     _world.BlockChanged += pos =>
        //     {
        //         ChunkBlockLight.Update(_world, pos);
        //         foreach (var direction in Directions.All)
        //             ChunkBlockLight.Update(_world, pos.Offset(direction));
        //     };
        // }
        //
        // public void Dispose()
        // {
        //     _world.Dispose();
        // }
        //
        // private void Tick()
        // {
        //     _input.Update();
        //     
        //     _player.UpdateMovement(_input);
        //     _player.UpdateHotbar(_input);
        //     
        //     var hit = Raycast.Cast(_rayCastContext, _player.GetCamera(0).Ray);
        //     _player.UpdateInteraction(_input, hit);
        // }

        public static async Task Main(string[] args)
        {
            var lifecycleEventBus = new EventBus();

            ModLoader.Instance.LoadMods();
            foreach (var mod in ModLoader.Instance.Mods)
                mod.AttachLifecycleEvents(lifecycleEventBus);

            GameRegistries.Initialize(lifecycleEventBus);

            if (args.Length == 1 && args[0] == "--server")
            {
                var server = new GameServer();
                Console.WriteLine("Server started!");
                server.Start();
                server.Running.Wait();
                Console.WriteLine("Server closed!");
                server.Dispose();
            }
            else
            {
                var client = new GameClient();
                Console.WriteLine("Started client!");
                client.Connect("localhost", 1234);
                Console.WriteLine("Connected to server!");
                client.State!.Ready.Wait();
                client.State!.Run();
                client.State!.Running.Wait();
                client.Dispose();
            }

            // var game = new Game();
            // await game._window.OpenWaitClosed();
            // game.Dispose();
        }
    }
}