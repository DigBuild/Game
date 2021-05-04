using System.Collections.Generic;
using DigBuild.Content.Registries;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Impl.Worlds;
using DigBuild.Engine.Math;
using DigBuild.Engine.Worlds;

namespace DigBuild.Content.Worldgen.Structure
{
    public class TreeStructure : IWorldgenStructure
    {
        private static readonly Vector3I OneChunk = new(15, 15, 15);
        public static Dictionary<Vector3I, Block> Blocks { get; } = new();

        static TreeStructure()
        {
            for (var y = 0; y < 6; y++)
                Blocks[(0, y, 0)] = GameBlocks.Log;
            
            for (var y = 3; y < 5; y++)
            for (var x = -2; x <= 2; x++)
            for (var z = -2; z <= 2; z++)
                Blocks.TryAdd((x, y, z), GameBlocks.Leaves);
            
            for (var x = -1; x <= 1; x++)
            for (var z = -1; z <= 1; z++)
                Blocks.TryAdd((x, 5, z), GameBlocks.Leaves);
            
            for (var x = -1; x <= 1; x++)
                Blocks.TryAdd((x, 6, 0), GameBlocks.Leaves);
            for (var z = -1; z <= 1; z++)
                Blocks.TryAdd((0, 6, z), GameBlocks.Leaves);

            Blocks.Remove((1, 5, 1));
            Blocks.Remove((-2, 3, 2));
        }

        public Vector3I Min { get; } = new(-2, 0, -2);
        public Vector3I Max { get; } = new(2, 7, 2);

        public void Place(Vector3I offset, IChunk chunk)
        {
            var min = Vector3I.Min(Vector3I.Max(Min + offset, Vector3I.Zero), OneChunk) - offset;
            var max = Vector3I.Min(Vector3I.Max(Max + offset, Vector3I.Zero), OneChunk) - offset;
            
            for (var x = min.X; x <= max.X; x++)
            for (var y = min.Y; y <= max.Y; y++)
            for (var z = min.Z; z <= max.Z; z++)
            {
                if (Blocks.TryGetValue((x, y, z), out var block))
                {
                    chunk.SetBlock(new ChunkBlockPos(x + offset.X, y + offset.Y, z + offset.Z), block);
                }
            }
        }
    }
}