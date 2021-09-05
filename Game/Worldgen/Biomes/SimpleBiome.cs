using System.Diagnostics.CodeAnalysis;
using DigBuild.Engine.Collections;
using DigBuild.Engine.Worldgen;

namespace DigBuild.Worldgen.Biomes
{
    /// <summary>
    /// A simple biome implementation.
    /// </summary>
    public sealed class SimpleBiome : IBiome
    {
        /// <summary>
        /// A set of worldgen attribute constraints.
        /// </summary>
        public IReadOnlyWorldgenRangeSet Constraints { get; init; } = new WorldgenRangeSet();

        /// <summary>
        /// A set of biome attributes.
        /// </summary>
        public IReadOnlyBiomeAttributeSet Attributes { get; init; } = new BiomeAttributeSet();
        
        public Grid<float> ComputeScores(ChunkDescriptionContext context)
        {
            return Constraints.GetScores(context);
        }

        public bool TryGet<T>(BiomeAttribute<T> attribute, [MaybeNullWhen(false)] out T value)
            where T : notnull
        {
            return Attributes.TryGet(attribute, out value);
        }
    }
}