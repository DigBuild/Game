using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using DigBuild.Blocks;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Math;
using DigBuild.Engine.Physics;
using DigBuild.Engine.Worlds;
using DigBuild.Engine.Worlds.Impl;
using DigBuild.Registries;

namespace DigBuild.Worlds
{
    /// <summary>
    /// A world-based ray casting context.
    /// </summary>
    public sealed class WorldRayCastContext : IGridAlignedRayCastingContext<WorldRayCastContext.Hit>
    {
        private readonly IWorld _world;

        public WorldRayCastContext(IWorld world)
        {
            _world = world;
        }

        public bool TryCollide(Vector3I gridPosition, Vector3 position, RayCaster.Ray ray, [NotNullWhen(true)] out Hit? hit)
        {
            var pos = new BlockPos(gridPosition);
            var block = _world.GetBlock(pos);
            if (block == null)
            {
                hit = null;
                return false;
            }

            var rayCollider = block.Get(_world, pos, GameBlockAttributes.RayCollider);
            if (!rayCollider.TryCollide(ray - (Vector3) gridPosition, out var colliderHit))
            {
                hit = null;
                return false;
            }
            
            hit = new Hit((Vector3) gridPosition, colliderHit.Side, colliderHit.Index, colliderHit.Bounds);
            return true;
        }

        /// <summary>
        /// A hit in the world.
        /// </summary>
        public sealed class Hit
        {
            /// <summary>
            /// The exact position.
            /// </summary>
            public Vector3 Position { get; }
            /// <summary>
            /// The block position.
            /// </summary>
            public BlockPos BlockPos { get; }
            /// <summary>
            /// The block face.
            /// </summary>
            public Direction Face { get; }
            /// <summary>
            /// The AABB index.
            /// </summary>
            public uint Index { get; }
            /// <summary>
            /// The bounding box.
            /// </summary>
            public AABB Bounds { get; }

            public Hit(Vector3 position, Direction face, uint index, AABB bounds)
            {
                Position = position;
                Face = face;
                Index = index;
                Bounds = bounds;
                BlockPos = new BlockPos(position);
            }
        }
    }
}