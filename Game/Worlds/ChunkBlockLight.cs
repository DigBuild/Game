using System;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Collections;
using DigBuild.Engine.Math;
using DigBuild.Engine.Serialization;
using DigBuild.Engine.Storage;
using DigBuild.Engine.Worlds;
using DigBuild.Engine.Worlds.Impl;
using DigBuild.Registries;

namespace DigBuild.Worlds
{
    public class ChunkBlockLight : IChunkBlockLight
    {
        private const uint ChunkSize = 16;

        // private readonly byte[,,] _values = new byte[ChunkSize, ChunkSize, ChunkSize];
        private readonly Octree<int>[] _values = new Octree<int>[WorldDimensions.ChunkVerticalSubdivisions];

        public event Action? Changed;

        public ChunkBlockLight()
        {
            for (var i = 0; i < _values.Length; i++)
                _values[i] = new Octree<int>(3, 0);
        }

        public byte Get(ChunkBlockPos pos)
        {
            var clusterValue = _values[pos.Y >> 4][pos.X >> 1, (pos.Y & 15) >> 1, pos.Z >> 1];
            return (byte) ((clusterValue >> (4 * (((pos.X & 1) << 2) | ((pos.Y & 1) << 1) | ((pos.Z & 1) << 0)))) & 0xF);
        }

        public void Set(ChunkBlockPos pos, byte value)
        {
            var clusterValue = _values[pos.Y >> 4][pos.X >> 1, (pos.Y & 15) >> 1, pos.Z >> 1];
            var position = 1 << (4 * (((pos.X & 1) << 2) | ((pos.Y & 1) << 1) | ((pos.Z & 1) << 0)));
            var newValue = (clusterValue & ~(0xF * position)) | (value * position);
            _values[pos.Y >> 4][pos.X >> 1, (pos.Y & 15) >> 1, pos.Z >> 1] = newValue;
        }
        
        public IChunkBlockLight Copy()
        {
            var copy = new ChunkBlockLight();
            for (var i = 0; i < WorldDimensions.ChunkVerticalSubdivisions; i++)
            for (var x = 0; x < ChunkSize; x++)
            for (var y = 0; y < ChunkSize; y++)
            for (var z = 0; z < ChunkSize; z++)
                copy._values[i][x, y, z] = _values[i][x, y, z];
            return copy;
        }

        private static byte GetCurrent(IReadOnlyWorld world, BlockPos pos)
        {
            return (world.GetChunk(pos.ChunkPos)?.Get(IChunkBlockLight.Type) as ChunkBlockLight)?.Get(pos.SubChunkPos) ?? 0;
        }

        private static byte Get(IReadOnlyWorld world, BlockPos pos, Direction direction)
        {
            var offset = pos.Offset(direction);
            var block = world.GetBlock(offset);
            if (block == null)
                return GetCurrent(world, offset);

            return block.Get(world, offset, BlockAttributes.LightEmission).Get(direction.GetOpposite());

        }
        
        public static byte Compute(IReadOnlyWorld world, BlockPos pos)
        {
            var block = world.GetBlock(pos);
            // TODO: Implement light absorption
            if (block != null)
                return block.Get(world, pos, BlockAttributes.LightEmission).Local;
            
            var negX = Get(world, pos, Direction.NegX);
            var posX = Get(world, pos, Direction.PosX);
            var negY = Get(world, pos, Direction.NegY);
            var posY = Get(world, pos, Direction.PosY);
            var negZ = Get(world, pos, Direction.NegZ);
            var posZ = Get(world, pos, Direction.PosZ);
            
            return (byte) (Math.Max(
                Math.Max(
                    Math.Max(negX, posX),
                    Math.Max(negY, posY)
                ),
                Math.Max(
                    Math.Max(negZ, posZ),
                    (byte) 1
                )
            ) - 1);
        }
        
        public static void Update(IWorld world, BlockPos pos)
        {
            var current = GetCurrent(world, pos);
            var computed = Compute(world, pos);

            if (current == computed)
                return;

            var chunk = world.GetChunk(pos.ChunkPos);
            if (chunk?.Get(IChunkBlockLight.Type) is not ChunkBlockLight storage)
                return;
            
            storage.Set(pos.SubChunkPos, computed);
            // ((World) world).ChunkManager.OnBlockChanged(pos);
            storage.Changed?.Invoke();
            
            foreach (var direction in Directions.All)
                Update(world, pos.Offset(direction));
        }

        public static ISerdes<IChunkBlockLight> Serdes { get; } = new SimpleSerdes<IChunkBlockLight>(
            (stream, light) => { },
            (stream, context) => new ChunkBlockLight()
        );
    }
}