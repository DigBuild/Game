using System.Collections.Generic;
using DigBuild.Engine.Math;
using DigBuild.Engine.Worldgen;

namespace DigBuild.Voxel
{
    public delegate void ChunkChangedEvent(Chunk chunk);

    public class ChunkManager
    {
        private readonly Dictionary<ChunkPos, Chunk> _chunks = new();

        private readonly WorldGenerator _generator;

        public event ChunkChangedEvent? ChunkChanged;

        public ChunkManager(WorldGenerator generator)
        {
            _generator = generator;
        }

        public Chunk? Get(ChunkPos pos, bool load = true)
        {
            if (pos.Y < 0) return null;

            _chunks.TryGetValue(pos, out var chunk);
            if (chunk != null || !load) return chunk;

            var slice = new Chunk[3];
            for (var y = 0; y < slice.Length; y++)
            {
                var p = new ChunkPos(pos.X, y, pos.Z);
                _chunks[p] = slice[y] = new Chunk(p);
            }
            _generator.GenerateSlice(new WorldSlicePos(pos.X, pos.Z), slice);
            if (pos.Y < slice.Length)
                return slice[pos.Y];

            return _chunks[pos] = new Chunk(pos);
        }

        public void OnBlockChanged(BlockPos pos)
        {
            ChunkChanged?.Invoke(_chunks[pos.ChunkPos]);
        }
    }
}