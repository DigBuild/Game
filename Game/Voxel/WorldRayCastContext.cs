using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using DigBuild.Engine.Math;
using DigBuild.Engine.Worlds;

namespace DigBuild.Voxel
{
    public sealed class WorldRayCastContext : IGridAlignedRayCastingContext<WorldRayCastContext.Hit>
    {
        private static readonly Vector3 Half = Vector3.One / 2;

        private readonly IWorld _world;

        public WorldRayCastContext(IWorld world)
        {
            _world = world;
        }

        public bool Visit(Vector3i gridPosition, Vector3 position, RayCaster.Ray ray, [NotNullWhen(true)] out Hit? hit)
        {
            if (_world.GetBlock(gridPosition) == null)
            {
                hit = null;
                return false;
            }

            var face = BlockFaces.FromOffset(position - gridPosition - Half);
            hit = new Hit(gridPosition, face);
            return true;
        }

        public sealed class Hit
        {
            public readonly Vector3 Position;
            public readonly BlockPos BlockPos;
            public readonly BlockFace Face;

            public Hit(Vector3 position, BlockFace face)
            {
                Position = position;
                Face = face;
                BlockPos = new BlockPos(position);
            }
        }
    }
}