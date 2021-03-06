using System;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Voxel;

namespace DigBuild.Voxel
{
    public class BlockChunkStorage : IBlockChunkStorage
    {
        public IBlockContainer Blocks { get; }
        public IBlockDataContainerContainer Data { get; }

        public BlockChunkStorage(Action notifyUpdate)
        {
            Blocks = new BlockContainer(notifyUpdate);
            Data = new BlockDataContainerContainer(notifyUpdate);
        }

        public sealed class BlockContainer : IBlockContainer
        {
            private readonly Action _notifyUpdate;
            private readonly Block?[,,] _blocks = new Block[16, 16, 16];

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
            private readonly BlockDataContainer?[,,] _data = new BlockDataContainer[16, 16, 16];

            public BlockDataContainerContainer(Action notifyUpdate)
            {
                _notifyUpdate = notifyUpdate;
                for (int x = 0; x < 16; x++)
                for (int y = 0; y < 16; y++)
                for (int z = 0; z < 16; z++)
                    _data[x, y, z] = new BlockDataContainer();
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