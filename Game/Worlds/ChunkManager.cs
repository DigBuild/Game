using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DigBuild.Engine.Math;
using DigBuild.Engine.Worldgen;
using DigBuild.Engine.Worlds;

namespace DigBuild.Worlds
{
    public delegate void ChunkChangedEvent(Chunk chunk);
    public delegate void ChunkUnloadedEvent(Chunk chunk);

    public class ChunkManager : IChunkManager, IDisposable
    {
        private readonly Dictionary<ChunkPos, Chunk> _chunks = new();
        private readonly Dictionary<ChunkPos, HashSet<LoadingTicket>> _tickets = new();

        private readonly WorldGenerator _generator;

        private readonly object _lockObject = new();
        private readonly Thread _generationThread;
        private bool _generationThreadActive = true;
        private HashSet<WorldSlicePos> _requestedChunks = new();
        private HashSet<WorldSlicePos> _requestedChunks2 = new();
        private readonly ConcurrentQueue<Chunk> _generatedChunks = new();
        private readonly ManualResetEventSlim _generatedChunksUpdateEvent = new();

        public event ChunkChangedEvent? ChunkChanged;
        public event ChunkUnloadedEvent? ChunkUnloaded;

        public ChunkManager(WorldGenerator generator, Func<ChunkPos> generationOriginGetter)
        {
            _generator = generator;
            _generationThread = new Thread(() =>
            {
                while (_generationThreadActive)
                {
                    lock (_lockObject)
                    {
                        _generatedChunksUpdateEvent.Reset();
                        (_requestedChunks2, _requestedChunks) = (_requestedChunks, _requestedChunks2);
                    }

                    var origin = generationOriginGetter();
                    var toGenerate = _requestedChunks2
                        .OrderBy(c => new Vector3I(c.X - origin.X, 0, c.Z - origin.Z).LengthSquared())
                        .ToList();
                    _requestedChunks2.Clear();

                    Parallel.ForEach(toGenerate, slicePos =>
                    {
                        var slice = GenerateSlice(slicePos);
                        foreach (var chunk in slice)
                            _generatedChunks.Enqueue(chunk);
                    });
                    
                    _generatedChunksUpdateEvent.Wait();
                }
            });
            _generationThread.Start();
        }

        public void Dispose()
        {
            _generationThreadActive = false;
            _generatedChunksUpdateEvent.Set();
            _generationThread.Join();
        }

        public void Update()
        {
            while (_generatedChunks.TryDequeue(out var chunk))
            {
                if (_chunks.TryAdd(chunk.Position, chunk))
                    ChunkChanged?.Invoke(chunk);
            }
        }

        public Chunk? Get(ChunkPos pos, bool load = true)
        {
            if (pos.Y < 0)
                return null;
            
            if (_chunks.TryGetValue(pos, out var chunk) || !load)
                return chunk;

            if (_chunks.ContainsKey(new ChunkPos(pos.X, 0, pos.Z)))
                return _chunks[pos] = new Chunk(pos);

            var slice = GenerateSlice(new WorldSlicePos(pos.X, pos.Z));
            foreach (var c in slice)
            {
                _chunks[c.Position] = c;
                ChunkChanged?.Invoke(c);
            }

            if (pos.Y < slice.Length)
                return slice[pos.Y];
            return _chunks[pos] = new Chunk(pos);
        }

        private Chunk[] GenerateSlice(WorldSlicePos pos)
        {
            var prototypes = _generator.GenerateSlice(pos);
            var slice = new Chunk[prototypes.Length];
            Parallel.For(0, slice.Length, y =>
            {
                var c = slice[y] = new Chunk(new ChunkPos(pos.X, y, pos.Z));
                c.CopyFrom(prototypes[y]);
            });
            return slice;
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
                    _requestedChunks.Add(new WorldSlicePos(pos.X, pos.Z));
                }
                tickets.Add((LoadingTicket) ticket);
            }

            lock (_lockObject)
            {
                _generatedChunksUpdateEvent.Set();
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