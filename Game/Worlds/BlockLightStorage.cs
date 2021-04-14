using System;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Impl.Worlds;
using DigBuild.Engine.Math;
using DigBuild.Engine.Worlds;
using DigBuild.Registries;

namespace DigBuild.Worlds
{
    public class ChunkChunkBlockLight : IChunkBlockLight
    {
        private const uint ChunkSize = 16;

        private readonly byte[,,] _values = new byte[ChunkSize, ChunkSize, ChunkSize];

        public event Action? Changed;

        public byte Get(ChunkBlockPosition pos)
        {
            return _values[pos.X, pos.Y, pos.Z];
        }
        
        public IChunkBlockLight Copy()
        {
            var copy = new ChunkChunkBlockLight();
            for (var x = 0; x < ChunkSize; x++)
            for (var y = 0; y < ChunkSize; y++)
            for (var z = 0; z < ChunkSize; z++)
                copy._values[x, y, z] = _values[x, y, z];
            return copy;
        }

        private static byte GetCurrent(IReadOnlyWorld world, BlockPos pos)
        {
            return (world.GetChunk(pos.ChunkPos)?.Get(IChunkBlockLight.Type) as ChunkChunkBlockLight)?.Get(pos.SubChunkPos) ?? 0;
        }

        private static byte Get(IReadOnlyWorld world, BlockPos pos, Direction direction)
        {
            var offset = pos.Offset(direction);
            var block = world.GetBlock(offset);
            if (block == null)
                return GetCurrent(world, offset);

            return block.Get(new ReadOnlyBlockContext(world, offset, block), BlockAttributes.LightEmission).Get(direction.GetOpposite());

        }
        
        public static byte Compute(IReadOnlyWorld world, BlockPos pos)
        {
            var block = world.GetBlock(pos);
            // TODO: Implement light absorption
            if (block != null)
                return block.Get(new ReadOnlyBlockContext(world, pos, block), BlockAttributes.LightEmission).Local;
            
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
            if (chunk?.Get(IChunkBlockLight.Type) is not ChunkChunkBlockLight storage)
                return;

            var sub = pos.SubChunkPos;
            storage._values[sub.X, sub.Y, sub.Z] = computed;
            // ((World) world).ChunkManager.OnBlockChanged(pos);
            storage.Changed?.Invoke();
            
            foreach (var direction in Directions.All)
                Update(world, pos.Offset(direction));
        }
    }
}