using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DigBuild.Engine.Collections;
using DigBuild.Engine.Math;
using DigBuild.Engine.Worldgen;

namespace DigBuild.Worldgen.Biomes
{
    /// <summary>
    /// A range set of worldgen attributes.
    /// </summary>
    public interface IReadOnlyWorldgenRangeSet : IEnumerable<KeyValuePair<IWorldgenAttribute, IRangeT>>
    {
        Grid<float> GetScores(ChunkDescriptionContext context);
    }

    public sealed class WorldgenRangeSet : IReadOnlyWorldgenRangeSet, ICollection<KeyValuePair<IWorldgenAttribute, IRangeT>>
    {
        private readonly Dictionary<IWorldgenAttribute, IRangeT> _ranges = new();
        private readonly Dictionary<IWorldgenAttribute, Func<ChunkDescriptionContext, Grid<float>>> _scoreProviders = new();

        public int Count => _ranges.Count;
        public bool IsReadOnly => false;

        /// <summary>
        /// Adds a range for a float grid attribute.
        /// </summary>
        /// <param name="attribute">The attribute</param>
        /// <param name="start">The range start</param>
        /// <param name="end">The range end</param>
        public void Add(WorldgenAttribute<Grid<float>> attribute, float start, float end)
        {
            _ranges.Add(attribute, new RangeT<float>(start, end));
            _scoreProviders.Add(attribute, context =>
            {
                var center = (end + start) / 2;
                var sigma = end - center;

                var values = context.Get(attribute);
                var scores = Grid<float>.Builder(WorldDimensions.ChunkWidth);
                for (var i = 0; i < scores.Size; i++)
                for (var j = 0; j < scores.Size; j++)
                {
                    var v = MathF.Abs(values[i, j] - center) / sigma;
                    scores[i, j] = -MathF.Pow(v, 3);
                }
                return scores.Build();
            });
        }
        
        /// <summary>
        /// Adds a range for an unsigned short grid attribute.
        /// </summary>
        /// <param name="attribute">The attribute</param>
        /// <param name="start">The range start</param>
        /// <param name="end">The range end</param>
        public void Add(WorldgenAttribute<Grid<ushort>> attribute, ushort start, ushort end)
        {
            _ranges.Add(attribute, new RangeT<ushort>(start, end));
            _scoreProviders.Add(attribute, context =>
            {
                var center = (end + start) / 2;
                var sigma = end - center;

                var values = context.Get(attribute);
                var scores = Grid<float>.Builder(WorldDimensions.ChunkWidth);
                for (var i = 0; i < scores.Size; i++)
                for (var j = 0; j < scores.Size; j++)
                {
                    var v = MathF.Abs(values[i, j] - center) / sigma;
                    scores[i, j] = -MathF.Pow(v, 3);
                }
                return scores.Build();
            });
        }
        
        public Grid<float> GetScores(ChunkDescriptionContext context)
        {
            var scores = Grid<float>.Builder(WorldDimensions.ChunkWidth);
            foreach (var provider in _scoreProviders.Values)
                scores.Add(provider(context));
            return scores.Build();
        }

        void ICollection<KeyValuePair<IWorldgenAttribute, IRangeT>>.Add(KeyValuePair<IWorldgenAttribute, IRangeT> item) {}
        void ICollection<KeyValuePair<IWorldgenAttribute, IRangeT>>.Clear() {}
        bool ICollection<KeyValuePair<IWorldgenAttribute, IRangeT>>.Contains(KeyValuePair<IWorldgenAttribute, IRangeT> item) => false;
        void ICollection<KeyValuePair<IWorldgenAttribute, IRangeT>>.CopyTo(KeyValuePair<IWorldgenAttribute, IRangeT>[] array, int arrayIndex) {}
        bool ICollection<KeyValuePair<IWorldgenAttribute, IRangeT>>.Remove(KeyValuePair<IWorldgenAttribute, IRangeT> item) => false;
        public IEnumerator<KeyValuePair<IWorldgenAttribute, IRangeT>> GetEnumerator() => _ranges.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}