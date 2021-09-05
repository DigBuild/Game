using DigBuild.Blocks;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Math;
using DigBuild.Engine.Physics;
using DigBuild.Engine.Registries;
using DigBuild.Platform.Resource;

namespace DigBuild.Registries
{
    /// <summary>
    /// The game's block attributes.
    /// </summary>
    public class GameBlockAttributes
    {
        /// <summary>
        /// A bounding box. Nullable. Defaults to a full block (1x1x1).
        /// </summary>
        public static BlockAttribute<AABB?> Bounds { get; private set; } = null!;
        /// <summary>
        /// A collider. Non-null. Defaults to a voxel collider using the <see cref="Bounds"/>.
        /// </summary>
        public static BlockAttribute<ICollider> Collider { get; private set; } = null!;
        /// <summary>
        /// A ray collider. Non-null. Defaults to a voxel ray collider using the <see cref="Bounds"/>.
        /// </summary>
        public static BlockAttribute<IRayCollider<VoxelRayCollider.Hit>> RayCollider { get; private set; } = null!;

        /// <summary>
        /// A light emission. Non-null. Defaults to 0.
        /// </summary>
        public static BlockAttribute<LightEmission> LightEmission { get; private set; } = null!;

        /// <summary>
        /// A direction. Nullable. Defaults to null.
        /// </summary>
        public static BlockAttribute<Direction?> Direction { get; private set; } = null!;
        /// <summary>
        /// A horizontal direction. Nullable. Defaults to null.
        /// </summary>
        public static BlockAttribute<Direction?> HorizontalDirection { get; private set; } = null!;
        
        /// <summary>
        /// Whether the block is to be treated like water. Non-null. Defaults to false.
        /// </summary>
        public static BlockAttribute<bool> Water { get; private set; } = null!;

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
            
            Water = registry.Register(
                new ResourceName(DigBuildGame.Domain, "water"),
                false
            );
        }
    }
}