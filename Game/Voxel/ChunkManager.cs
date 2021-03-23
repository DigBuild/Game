using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DigBuild.Engine.Math;
using DigBuild.Engine.Worldgen;
using DigBuild.Engine.Worlds;

namespace DigBuild.Voxel
{
    public delegate void ChunkChangedEvent(Chunk chunk);
    public delegate void ChunkUnloadedEvent(Chunk chunk);

    public class ChunkManager : IChunkManager
    {
        private readonly Dictionary<ChunkPos, Chunk> _chunks = new();

        private readonly Dictionary<ChunkPos, HashSet<LoadingTicket>> _tickets = new();

        private readonly WorldGenerator _generator;

        public event ChunkChangedEvent? ChunkChanged;
        public event ChunkUnloadedEvent? ChunkUnloaded;

        public ChunkManager(WorldGenerator generator)
        {
            _generator = generator;
        }

        public Chunk? Get(ChunkPos pos, bool load = true)
        {
            if (pos.Y < 0) return null;
            
            if (_chunks.TryGetValue(pos, out var chunk) || !load)
                return chunk;

            var slice = new Chunk[3];
            for (var y = 0; y < slice.Length; y++)
            {
                var p = new ChunkPos(pos.X, y, pos.Z);
                _chunks[p] = slice[y] = new Chunk(p);
            }
            _generator.GenerateSlice(new WorldSlicePos(pos.X, pos.Z), slice);
            foreach (var c in slice)
                ChunkChanged?.Invoke(c);

            if (pos.Y < slice.Length)
                return slice[pos.Y];

            return _chunks[pos] = new Chunk(pos);
        }

        public void OnBlockChanged(BlockPos pos)
        {
            ChunkChanged?.Invoke(_chunks[pos.ChunkPos]);
        }

        public bool RequestLoadingTicket([MaybeNullWhen(false)] out IChunkLoadingTicket ticket, IEnumerable<ChunkPos> chunkPositions)
        {
            var positions = chunkPositions.ToImmutableHashSet();
            ticket = new LoadingTicket(this, positions);
            foreach (var pos in positions)
            {
                if (!_tickets.TryGetValue(pos, out var tickets))
                {
                    _tickets[pos] = tickets = new HashSet<LoadingTicket>();
                    Get(pos);
                }
                tickets.Add((LoadingTicket) ticket);
            }
            return true;
        }

        private void Unload(ChunkPos pos)
        {
            _chunks.Remove(pos, out var chunk);
            _tickets.Remove(pos);

            if (chunk != null)
                ChunkUnloaded?.Invoke(chunk);
        }

        private sealed class LoadingTicket : IChunkLoadingTicket
        {
            private readonly ChunkManager _chunkManager;
            private readonly ImmutableHashSet<ChunkPos> _positions;

            public LoadingTicket(ChunkManager chunkManager, ImmutableHashSet<ChunkPos> positions)
            {
                _chunkManager = chunkManager;
                _positions = positions;
            }

            public void Release()
            {
                foreach (var pos in _positions)
                {
                    var tickets = _chunkManager._tickets[pos];
                    tickets.Remove(this);
                    if (tickets.Count == 0)
                    {
                        _chunkManager.Unload(pos);
                    }
                }
            }
        }
    }
}