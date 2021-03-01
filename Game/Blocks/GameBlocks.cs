using System;
using System.Numerics;
using DigBuild.Engine.Math;
using DigBuild.Engine.Reg;
using DigBuild.Engine.Voxel;
using DigBuild.Platform.Resource;

namespace DigBuild.Blocks
{
    public static class GameBlocks
    {
        public static Block Terrain { get; private set; } = null!;
        public static Block Water { get; private set; } = null!;

        // public static Block ClimbBlock { get; private set; } = null!;

        // public static Block CountingBlock { get; private set; } = null!;

        internal static void Register(RegistryBuilder<Block> builder)
        {
            Terrain = builder.CreateTmp(new ResourceName(Game.Domain, "terrain"), 0, new VoxelCollider(), true);
            Water = builder.CreateTmp(new ResourceName(Game.Domain, "water"), 1, new VoxelCollider(), false);

            // ClimbBlock = builder.CreateTmp(new ResourceName(Game.Domain, "climb_block"), 2, new ClimbingCollider());

            // CountingBlock = builder.Create(new ResourceName(Game.Domain, "counting_block"), builder =>
            // {
            //     var data = builder.Add<CountingBehaviorData>();
            //     builder.Attach(new CountingBehavior(), data);
            // });
        }
    }

    // public class ClimbingCollider : ICollider
    // {
    //     private readonly ICollider _collider = new VoxelCollider(AABB.FullBlock);
    //
    //     public bool Collide(AABB target, Vector3 motion, out Vector3 intersection)
    //     {
    //         if (!_collider.Collide(target, motion, out intersection))
    //             return false;
    //         if (intersection.Y < 0)
    //             return true;
    //         intersection = new Vector3(intersection.X, MathF.Abs(Sum(intersection)), intersection.Z);
    //         return true;
    //     }
    //     private static float Sum(Vector3 vec) => vec.X + vec.Y + vec.Z;
    // }
}