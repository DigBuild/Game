using DigBuild.Engine.Blocks;
using DigBuild.Engine.Collections;
using DigBuild.Engine.Math;
using DigBuild.Engine.Worldgen;

namespace DigBuild.Worldgen.Biomes
{
    public sealed class SimpleBiome : IBiome
    {
        public Block SurfaceBlock { get; init; } = null!;

        public IReadOnlyWorldgenRangeSet EnvironmentConstraints { get; init; } = new WorldgenRangeSet();
        public IReadOnlyWorldgenRangeSet GenerationConstraints { get; init; } = new WorldgenRangeSet();
        
        public Grid<float> GetScores(WorldSliceDescriptionContext context)
        {
            return EnvironmentConstraints.GetScores(context);
        }

        public RangeT<T>? GetConstraints<T>(WorldgenAttribute<Grid<T>> attribute)
        {
            return GenerationConstraints.Get(attribute);
        }
    }
}