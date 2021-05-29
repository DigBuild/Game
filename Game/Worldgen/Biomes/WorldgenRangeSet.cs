using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DigBuild.Engine.Collections;
using DigBuild.Engine.Math;
using DigBuild.Engine.Worldgen;

namespace DigBuild.Worldgen.Biomes
{
    public interface IReadOnlyWorldgenRangeSet : IEnumerable<KeyValuePair<IWorldgenAttribute, IRangeT>>
    {
        int Count { get; }

        RangeT<T>? Get<T>(WorldgenAttribute<Grid<T>> attribute);
        bool TryGet<T>(WorldgenAttribute<Grid<T>> attribute, [MaybeNullWhen(false)] out RangeT<T> range);

        Grid<float> GetScores(WorldSliceDescriptionContext context);
    }

    public sealed class WorldgenRangeSet : IReadOnlyWorldgenRangeSet, ICollection<KeyValuePair<IWorldgenAttribute, IRangeT>>
    {
        private readonly Dictionary<IWorldgenAttribute, IRangeT> _ranges = new();
        private readonly Dictionary<IWorldgenAttribute, Func<WorldSliceDescriptionContext, Grid<float>>> _scoreProviders = new();

        public int Count => _ranges.Count;
        public bool IsReadOnly => false;

        public void Add(WorldgenAttribute<Grid<float>> attribute, float start, float end)
        {
            _ranges.Add(attribute, new RangeT<float>(start, end));
            _scoreProviders.Add(attribute, context =>
            {
                var center = (end + start) / 2;
                var sigma = end - center;

                var values = context.Get(attribute);
                var scores = Grid<float>.Builder(WorldDimensions.ChunkSize);
                for(var i = 0; i < scores.Size; i++)
                for (var j = 0; j < scores.Size; j++)
                {
                    var v = MathF.Abs(values[i, j] - center) / sigma;
                    scores[i, j] = -MathF.Pow(v, 3);
                }
                return scores.Build();
            });
        }

        public void Add(WorldgenAttribute<Grid<ushort>> attribute, ushort start, ushort end)
        {
            _ranges.Add(attribute, new RangeT<ushort>(start, end));
            _scoreProviders.Add(attribute, context =>
            {
                var center = (end + start) / 2;
                var sigma = end - center;

                var values = context.Get(attribute);
                var scores = Grid<float>.Builder(WorldDimensions.ChunkSize);
                for(var i = 0; i < scores.Size; i++)
                for (var j = 0; j < scores.Size; j++)
                {
                    var v = MathF.Abs(values[i, j] - center) / sigma;
                    scores[i, j] = -MathF.Pow(v, 3);
                }
                return scores.Build();
            });
        }

        public RangeT<T>? Get<T>(WorldgenAttribute<Grid<T>> attribute)
        {
            return _ranges.TryGetValue(attribute, out var range) ? (RangeT<T>) range : null;
        }

        public bool TryGet<T>(WorldgenAttribute<Grid<T>> attribute, [MaybeNullWhen(false)] out RangeT<T> range)
        {
            if (_ranges.TryGetValue(attribute, out var r))
            {
                range = (RangeT<T>) r;
                return true;
            }

            range = default;
            return false;
        }

        public Grid<float> GetScores(WorldSliceDescriptionContext context)
        {
            var scores = Grid<float>.Builder(WorldDimensions.ChunkSize);
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