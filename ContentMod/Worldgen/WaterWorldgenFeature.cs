using System;
using System.Collections.Immutable;
using DigBuild.Content.Registries;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Math;
using DigBuild.Engine.Worldgen;
using DigBuild.Engine.Worlds;
using DigBuild.Engine.Worlds.Impl;
using IWorldgenAttribute = DigBuild.Engine.Worldgen.IWorldgenAttribute;
using IWorldgenFeature = DigBuild.Engine.Worldgen.IWorldgenFeature;

namespace DigBuild.Content.Worldgen
{
    public class WaterWorldgenFeature : IWorldgenFeature
    {
        private const uint ChunkSize = 16;
        private const ushort Threshold = 12;
        
        private readonly Block _waterBlock;
        private readonly Block _iceBlock;
        
        public WaterWorldgenFeature(Block waterBlock, Block iceBlock)
        {
            _waterBlock = waterBlock;
            _iceBlock = iceBlock;
        }

        public void Describe(ChunkDescriptionContext context)
        {
        }

        public void Populate(ChunkDescriptor descriptor, IChunk chunk)
        {
            var height = descriptor.Get(WorldgenAttributes.TerrainHeight);
            var temperature = descriptor.Get(WorldgenAttributes.Temperature);

            for (var x = 0; x < ChunkSize; x++)
            for (var z = 0; z < ChunkSize; z++)
            {
                if (height[x, z] > Threshold)
                    continue;

                var localHeight = Math.Min(Threshold, ChunkSize);
                for (var y = 0; y <= localHeight - 1; y++)
                {
                    var block = temperature[x, z] < 0.4f ? _iceBlock : _waterBlock;

                    var pos = new ChunkBlockPos(x, y, z);
                    if (chunk.GetBlock(pos) == null)
                        chunk.SetBlock(pos, block);
                }
            }
        }
    }
}