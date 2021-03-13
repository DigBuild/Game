using DigBuild.Engine.Items;
using DigBuild.Engine.Voxel;

namespace DigBuild.Items
{
    public interface IPlayerItemContext : IItemContext
    {
        public IWorld World { get; }
    }

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