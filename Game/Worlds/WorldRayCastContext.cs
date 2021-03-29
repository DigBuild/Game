using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using DigBuild.Blocks;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Math;
using DigBuild.Engine.Physics;
using DigBuild.Engine.Worlds;
using DigBuild.Registries;

namespace DigBuild.Worlds
{
    public sealed class WorldRayCastContext : IGridAlignedRayCastingContext<WorldRayCastContext.Hit>
    {
        private readonly IWorld _world;

        public WorldRayCastContext(IWorld world)
        {
            _world = world;
        }

        public bool Visit(Vector3I gridPosition, Vector3 position, Raycast.Ray ray, [NotNullWhen(true)] out Hit? hit)
        {
            var pos = new BlockPos(gridPosition);
            var block = _world.GetBlock(pos);
            if (block == null)
            {
                hit = null;
                return false;
            }

            var rayCollider = block.Get(new BlockContext(_world, pos, block), BlockAttributes.RayCollider);
            if (!rayCollider.TryCollide(ray - (Vector3) gridPosition, out var colliderHit))
            {
                hit = null;
                return false;
            }
            
            hit = new Hit((Vector3) gridPosition, colliderHit.Side, colliderHit.Index, colliderHit.Bounds);
            return true;
        }

        public sealed class Hit
        {
            public readonly Vector3 Position;
            public readonly BlockPos BlockPos;
            public readonly Direction Face;
            public readonly uint Index;
            public readonly AABB Bounds;

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