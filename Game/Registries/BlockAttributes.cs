using DigBuild.Blocks;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Math;
using DigBuild.Engine.Physics;
using DigBuild.Engine.Registries;
using DigBuild.Platform.Resource;

namespace DigBuild.Registries
{
    public class BlockAttributes
    {
        public static BlockAttribute<AABB?> Bounds { get; private set; } = null!;
        public static BlockAttribute<ICollider> Collider { get; private set; } = null!;
        public static BlockAttribute<IRayCollider<VoxelRayCollider.Hit>> RayCollider { get; private set; } = null!;

        public static BlockAttribute<LightEmission> LightEmission { get; private set; } = null!;

        public static BlockAttribute<Direction?> Direction { get; private set; } = null!;
        public static BlockAttribute<Direction?> HorizontalDirection { get; private set; } = null!;

        internal static void Register(RegistryBuilder<IBlockAttribute> registry)
        {
            Bounds = registry.Register<AABB?>(
                new ResourceName(DigBuildGame.Domain, "bounds"),
                AABB.FullBlock
            );
            Collider = registry.Register(
                new ResourceName(DigBuildGame.Domain, "collider"),
                ctx =>
                {
                    var bounds = ctx.Block.Get(ctx, Bounds);
                    return bounds.HasValue ? new VoxelCollider(bounds.Value) : ICollider.None;
                }
            );
            RayCollider = registry.Register(
                new ResourceName(DigBuildGame.Domain, "ray_collider"),
                ctx =>
                {
                    var bounds = ctx.Block.Get(ctx, Bounds);
                    return bounds.HasValue ? new VoxelRayCollider(bounds.Value) : IRayCollider<VoxelRayCollider.Hit>.None;
                }
            );
            
            LightEmission = registry.Register(
                new ResourceName(DigBuildGame.Domain, "light_emission"),
                new LightEmission(0)
            );

            Direction = registry.Register(
                new ResourceName(DigBuildGame.Domain, "direction"),
                (Direction?) null
            );
            HorizontalDirection = registry.Register(
                new ResourceName(DigBuildGame.Domain, "horizontal_direction"),
                (Direction?) null
            );
        }
    }
}