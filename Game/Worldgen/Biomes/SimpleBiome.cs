using System.Diagnostics.CodeAnalysis;
using DigBuild.Engine.Collections;
using DigBuild.Engine.Worldgen;

namespace DigBuild.Worldgen.Biomes
{
    public sealed class SimpleBiome : IBiome
    {
        public IReadOnlyWorldgenRangeSet Constraints { get; init; } = new WorldgenRangeSet();
        public IReadOnlyBiomeAttributeSet Attributes { get; init; } = new BiomeAttributeSet();
        
        public Grid<float> GetScores(WorldSliceDescriptionContext context)
        {
            return Constraints.GetScores(context);
        }

        public bool TryGetAttribute<T>(BiomeAttribute<T> attribute, [MaybeNullWhen(false)] out T value)
            where T : notnull
        {
            return Attributes.TryGet(attribute, out value);
        }
    }
}