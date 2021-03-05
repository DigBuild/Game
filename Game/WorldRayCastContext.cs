using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using DigBuild.Engine.Math;
using DigBuild.Engine.Voxel;

namespace DigBuild
{
    public sealed class WorldRayCastContext : IGridAlignedRayCastingContext<WorldRayCastContext.Hit>
    {
        private readonly IWorld _world;

        public WorldRayCastContext(IWorld world)
        {
            _world = world;
        }

        public bool Visit(Vector3i position, RayCaster.Ray ray, [NotNullWhen(true)] out Hit? hit)
        {
            if (_world.GetBlock(position) == null)
            {
                hit = null;
                return false;
            }
            hit = new Hit(position);
            return true;
        }

        public sealed class Hit
        {
            public readonly Vector3 Position;

            public Hit(Vector3 position)
            {
                Position = position;
            }
        }
    }
}