using DigBuild.Behaviors;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Registries;
using DigBuild.Platform.Resource;

namespace DigBuild.Registries
{
    public class BlockCapabilities
    {
        public static BlockCapability<IInternalMultiblock?> InternalMultiblock { get; private set; } = null!;
        
        internal static void Register(RegistryBuilder<IBlockCapability> registry)
        {
            InternalMultiblock = registry.Register(
                new ResourceName(Game.Domain, "internal/multiblock"),
                (IInternalMultiblock?) null
            );
        }
    }
}