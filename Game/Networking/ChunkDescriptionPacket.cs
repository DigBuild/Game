using DigBuild.Client;
using DigBuild.Engine.Impl.Worlds;
using DigBuild.Engine.Networking;
using DigBuild.Engine.Serialization;

namespace DigBuild.Networking
{
    public class ChunkDescriptionPacket : IPacket
    {
        public Chunk Chunk { get; set; } = null!;

        public void Handle(IConnection connection)
        {
            GameClient.Instance!.State!.World.Add(Chunk);
            GameClient.Instance!.State!.World.GetChunk(Chunk.Position);
        }

        public static ISerdes<ChunkDescriptionPacket> Serdes = new CompositeSerdes<ChunkDescriptionPacket>()
        {
            {1u, p => p.Chunk, Chunk.Serdes}
        };
    }
}