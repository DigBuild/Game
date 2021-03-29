using DigBuild.Engine.Items;
using DigBuild.Engine.Worlds;

namespace DigBuild.Items
{
    public interface IPlayerItemContext : IItemContext
    {
        public IWorld World { get; }
    }
}