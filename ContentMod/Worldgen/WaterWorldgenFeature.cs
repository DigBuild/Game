using System;
using System.Collections.Immutable;
using DigBuild.Content.Registries;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Collections;
using DigBuild.Engine.Impl.Worlds;
using DigBuild.Engine.Math;
using DigBuild.Engine.Worldgen;
using DigBuild.Engine.Worlds;
using IWorldgenAttribute = DigBuild.Engine.Worldgen.IWorldgenAttribute;
using IWorldgenFeature = DigBuild.Engine.Worldgen.IWorldgenFeature;

namespace DigBuild.Content.Worldgen
{
    public class WaterWorldgenFeature : IWorldgenFeature
    {
        private const uint ChunkSize = 16;
        private const ushort Threshold = 12;

        private readonly Block _waterBlock;

        public IImmutableSet<IWorldgenAttribute> InputAttributes => ImmutableHashSet.Create<IWorldgenAttribute>(
            WorldgenAttributes.TerrainHeight
        );

        public IImmutableSet<IWorldgenAttribute> OutputAttributes => ImmutableHashSet.Create<IWorldgenAttribute>(
        );

        public WaterWorldgenFeature(Block waterBlock)
        {
            _waterBlock = waterBlock;
        }

        public void DescribeSlice(WorldSliceDescriptionContext context)
        {
        }

        public void PopulateChunk(WorldSliceDescriptor descriptor, IChunk chunk)
        {
            var height = descriptor.Get(WorldgenAttributes.TerrainHeight);

            for (var x = 0; x < ChunkSize; x++)
            for (var z = 0; z < ChunkSize; z++)
            {
                if (height[x, z] > Threshold)
                    continue;

                var relativeHeight = Threshold - chunk.Position.Y * ChunkSize;
                if (relativeHeight <= 0)
                    continue;
                var localHeight = Math.Min(relativeHeight, ChunkSize);
                for (var y = 0; y <= localHeight - 1; y++)
                {
                    var pos = new ChunkBlockPos(x, y, z);
                    if (chunk.GetBlock(pos) == null)
                        chunk.SetBlock(pos, _waterBlock);
                }
            }
        }
    }
}