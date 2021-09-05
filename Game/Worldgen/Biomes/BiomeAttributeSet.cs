using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DigBuild.Worldgen.Biomes
{
    /// <summary>
    /// A set of biome attributes and their values.
    /// </summary>
    public interface IReadOnlyBiomeAttributeSet : IEnumerable<KeyValuePair<IBiomeAttribute, object>>
    {
        /// <summary>
        /// The amount of attributes.
        /// </summary>
        int Count { get; }
        
        /// <summary>
        /// Tries to get the value of an attribute.
        /// </summary>
        /// <typeparam name="T">The attribute type</typeparam>
        /// <param name="attribute">The attribute</param>
        /// <param name="value">The value</param>
        /// <returns>Whether it was found or not</returns>
        bool TryGet<T>(BiomeAttribute<T> attribute, [MaybeNullWhen(false)] out T value)
            where T : notnull;
    }

    /// <summary>
    /// A dictionary-backed collection of biome attributes and their values.
    /// </summary>
    public sealed class BiomeAttributeSet : IReadOnlyBiomeAttributeSet, ICollection<KeyValuePair<IBiomeAttribute, object>>
    {
        private readonly Dictionary<IBiomeAttribute, object> _values = new();

        public int Count => _values.Count;
        public bool IsReadOnly => false;
        
        /// <summary>
        /// Adds a new attribute and value pair.
        /// </summary>
        /// <typeparam name="T">The attribute type</typeparam>
        /// <param name="attribute">The attribute</param>
        /// <param name="value">The value</param>
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