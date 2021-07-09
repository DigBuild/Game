using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using DigBuild.Engine.Collections;
using DigBuild.Engine.Impl.Worlds;
using DigBuild.Engine.Math;
using DigBuild.Engine.Storage;
using DigBuild.Engine.Worlds;

namespace DigBuild.Worlds
{
    public sealed class RegionStorage : IRegionStorage
    {
        private readonly LockStore<RegionChunkPos> _locks = new();

        public RegionPos Position { get; }

        public RegionStorage(RegionPos position)
        {
            Position = position;
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
                chunk = Chunk.Serdes.Deserialize(stream);
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
            using var lck = _locks.Lock(chunk.Position.RegionChunkPos);

            var stream = File.Create(GetPath(chunk.Position.RegionChunkPos));
            Chunk.Serdes.Serialize(stream, chunk);
            stream.Flush();
            stream.Close();
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