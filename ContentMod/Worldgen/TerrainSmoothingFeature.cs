using System.Collections.Immutable;
using System.Linq;
using DigBuild.Content.Registries;
using DigBuild.Engine.Collections;
using DigBuild.Engine.Math;
using DigBuild.Engine.Worldgen;
using DigBuild.Engine.Worlds;

namespace DigBuild.Content.Worldgen
{
    public class TerrainSmoothingFeature : IWorldgenFeature
    {
        private static readonly float[] BlurKernel =
        {
            0.07130343198685299f,
            0.13151412084312236f,
            0.18987923288883810f,
            0.21460642856237300f,
            0.18987923288883810f,
            0.13151412084312236f,
            0.07130343198685299f,
        };
        private static readonly int BlurStart = -(BlurKernel.Length - 1) / 2;
        
        public void Describe(ChunkDescriptionContext context)
        {
            var inTerrainHeight = context.GetExtendedGrid(WorldgenAttributes.TerrainHeight);
            var terrainHeight = Grid<ushort>.Builder(WorldDimensions.ChunkSize);

            // Apply blur on X
            for (var x = 0; x < terrainHeight.Size; x++)
            for (var z = 0; z < terrainHeight.Size; z++)
                terrainHeight[x, z] = (ushort) BlurKernel.Select((t, i) => inTerrainHeight[x + BlurStart + i, z] * t).Sum();

            var xBlurTerrainHeight = new ExtendedWorldgenGrid<ushort>(terrainHeight.Build(), inTerrainHeight);

            // Apply blur on Z
            for (var x = 0; x < terrainHeight.Size; x++)
            for (var z = 0; z < terrainHeight.Size; z++)
                terrainHeight[x, z] = (ushort) BlurKernel.Select((t, i) => xBlurTerrainHeight[x, z + BlurStart + i] * t).Sum();

            context.Submit(WorldgenAttributes.TerrainHeight, terrainHeight.Build());
        }

        public void Populate(ChunkDescriptor descriptor, IChunk chunk)
        {
        }
    }
}