using System;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Voxel;

namespace DigBuild.Voxel
{
    public class BlockChunkStorage : IBlockChunkStorage
    {
        private const uint ChunkSize = 16;

        public IBlockContainer Blocks { get; }
        public IBlockDataContainerContainer Data { get; }

        public BlockChunkStorage(Action notifyUpdate)
        {
            Blocks = new BlockContainer(notifyUpdate);
            Data = new BlockDataContainerContainer(notifyUpdate);
        }

        public void CopyFrom(IBlockChunkStorage other)
        {
            if (other is not BlockChunkStorage bcs)
                throw new ArgumentException("Incompatible storage.");
            
            for (var x = 0; x < ChunkSize; x++)
            for (var y = 0; y < ChunkSize; y++)
            for (var z = 0; z < ChunkSize; z++)
            {
                Blocks[x, y, z] = bcs.Blocks[x, y, z];
                Data[x, y, z] = bcs.Data[x, y, z];
            }
        }

        public sealed class BlockContainer : IBlockContainer
        {
            private readonly Action _notifyUpdate;
            private readonly Block?[,,] _blocks = new Block[ChunkSize, ChunkSize, ChunkSize];

            public BlockContainer(Action notifyUpdate)
            {
                _notifyUpdate = notifyUpdate;
            }

            public Block? this[int x, int y, int z]
            {
                get => _blocks[x, y, z];
                set
                {
                    _blocks[x, y, z] = value;
                    _notifyUpdate();
                }
            }
        }

        public sealed class BlockDataContainerContainer : IBlockDataContainerContainer
        {
            private readonly Action _notifyUpdate;
            private readonly BlockDataContainer?[,,] _data = new BlockDataContainer[ChunkSize, ChunkSize, ChunkSize];

            public BlockDataContainerContainer(Action notifyUpdate)
            {
                _notifyUpdate = notifyUpdate;
            }

            public BlockDataContainer? this[int x, int y, int z]
            {
                get => _data[x, y, z];
                set
                {
                    _data[x, y, z] = value;
                    _notifyUpdate();
                }
            }
        }
    }
}