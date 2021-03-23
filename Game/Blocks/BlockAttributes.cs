using DigBuild.Engine.Blocks;
using DigBuild.Engine.Math;
using DigBuild.Engine.Registries;
using DigBuild.Platform.Resource;

namespace DigBuild.Blocks
{
    public class BlockAttributes
    {
        public static BlockAttribute<ICollider> Collider { get; private set; } = null!;
        
        internal static void Register(RegistryBuilder<IBlockAttribute> registry)
        {
            Collider = registry.Register<ICollider>(
                new ResourceName(Game.Domain, "collider"),
                new VoxelCollider(AABB.FullBlock)
            );
        }
    }
}