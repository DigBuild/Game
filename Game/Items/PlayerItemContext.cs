using DigBuild.Engine.Items;
using DigBuild.Engine.Worlds;

namespace DigBuild.Items
{
    public sealed class PlayerItemContext : IPlayerItemContext
    {
        public ItemInstance Instance { get; }

        public IWorld World { get; }

        public PlayerItemContext(ItemInstance instance, IWorld world)
        {
            Instance = instance;
            World = world;
        }
    }
}