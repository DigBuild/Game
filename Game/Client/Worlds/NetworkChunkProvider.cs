using System.Diagnostics.CodeAnalysis;
using DigBuild.Engine.Impl.Worlds;
using DigBuild.Engine.Math;
using DigBuild.Engine.Ticking;
using DigBuild.Engine.Utils;

namespace DigBuild.Client.Worlds
{
    public sealed class NetworkChunkProvider : IChunkProvider
    {
        public const ulong ChunkExpirationDelay = 20;

        private readonly Cache<ChunkPos, Chunk> _receivedChunks;

        public NetworkChunkProvider(ITickSource tickSource)
        {
            _receivedChunks = new Cache<ChunkPos, Chunk>(tickSource, ChunkExpirationDelay);
        }

        public void Add(Chunk chunk)
        {
            _receivedChunks[chunk.Position] = chunk;
        }

        public bool TryGet(ChunkPos pos, [NotNullWhen(true)] out Chunk? chunk)
        {
            return _receivedChunks.Remove(pos, out chunk);
        }
    }
}