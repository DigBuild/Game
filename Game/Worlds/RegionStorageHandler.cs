using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using DigBuild.Engine.Collections;
using DigBuild.Engine.Math;
using DigBuild.Engine.Serialization;
using DigBuild.Engine.Storage;
using DigBuild.Engine.Ticking;
using DigBuild.Engine.Worlds;
using DigBuild.Engine.Worlds.Impl;

namespace DigBuild.Worlds
{
    public sealed class RegionStorageHandler : IRegionStorageHandler
    {
        private readonly IWorld _world;
        private readonly LockStore<RegionChunkPos> _locks = new();

        private readonly HashSet<Chunk> _savedChunks = new();

        public RegionPos Position { get; }

        public RegionStorageHandler(IWorld world, RegionPos position, ITickSource tickSource)
        {
            _world = world;
            Position = position;
            tickSource.Tick += Tick;
        }

        private void Tick()
        {
            lock (_savedChunks)
            {
                foreach (var chunk in _savedChunks)
                {
                    using var lck = _locks.Lock(chunk.Position.RegionChunkPos);

                    var stream = File.Create(GetPath(chunk.Position.RegionChunkPos));
                    Chunk.Serdes.Serialize(stream, chunk);
                    stream.Flush();
                    stream.Close();
                }

                _savedChunks.Clear();   
            }
        }

        public bool TryLoad(RegionChunkPos pos, [NotNullWhen(true)] out Chunk? chunk)
        {
            using var lck = _locks.Lock(pos);

            try
            {
                var path = GetPath(pos);

                if (!File.Exists(path))
                {
                    chunk = null;
                    return false;
                }

                var stream = File.OpenRead(path);
                chunk = Chunk.Serdes.Deserialize(stream, new SimpleDeserializationContext()
                {
                    _world
                });
                stream.Close();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"There was an error while loading chunk at {Position + pos}. Skipping. Error: {e.Message}");
                chunk = null;
                return false;
            }
        }

        public void Save(Chunk chunk)
        {
            lock (_savedChunks)
                _savedChunks.Add(chunk);
        }

        private string GetPath(RegionChunkPos pos)
        {
            var path = $"world/region/{Position.X}.{Position.Z}/chunk/{pos.X}.{pos.Z}.bin";
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            return path;
        }

        public DataContainer<IRegion> LoadOrCreateManagedData()
        {
            return new DataContainer<IRegion>();
        }
    }
}