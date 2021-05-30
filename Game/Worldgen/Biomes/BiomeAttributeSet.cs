using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DigBuild.Worldgen.Biomes
{
    public interface IReadOnlyBiomeAttributeSet : IEnumerable<KeyValuePair<IBiomeAttribute, object>>
    {
        int Count { get; }
        
        bool TryGet<T>(BiomeAttribute<T> attribute, [MaybeNullWhen(false)] out T value)
            where T : notnull;
    }

    public sealed class BiomeAttributeSet : IReadOnlyBiomeAttributeSet, ICollection<KeyValuePair<IBiomeAttribute, object>>
    {
        private readonly Dictionary<IBiomeAttribute, object> _values = new();

        public int Count => _values.Count;
        public bool IsReadOnly => false;
        
        public void Add<T>(BiomeAttribute<T> attribute, T value)
            where T : notnull
        {
            _values.Add(attribute, value);
        }

        public bool TryGet<T>(BiomeAttribute<T> attribute, [MaybeNullWhen(false)] out T value)
            where T : notnull
        {
            if (_values.TryGetValue(attribute, out var v))
            {
                value = (T) v;
                return true;
            }

            value = default;
            return false;
        }
        
        void ICollection<KeyValuePair<IBiomeAttribute, object>>.Add(KeyValuePair<IBiomeAttribute, object> item) {}
        void ICollection<KeyValuePair<IBiomeAttribute, object>>.Clear() {}
        bool ICollection<KeyValuePair<IBiomeAttribute, object>>.Contains(KeyValuePair<IBiomeAttribute, object> item) => false;
        void ICollection<KeyValuePair<IBiomeAttribute, object>>.CopyTo(KeyValuePair<IBiomeAttribute, object>[] array, int arrayIndex) {}
        bool ICollection<KeyValuePair<IBiomeAttribute, object>>.Remove(KeyValuePair<IBiomeAttribute, object> item) => false;
        public IEnumerator<KeyValuePair<IBiomeAttribute, object>> GetEnumerator() => _values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}