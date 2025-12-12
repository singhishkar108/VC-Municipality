using System;
using System.Collections;
using System.Collections.Generic;

namespace MunicipalityApp.DataStructures
{
    // Fully custom hash table with separate chaining
    public class SimpleHashTable<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private class Node
        {
            public TKey Key;
            public TValue Value;
            public Node? Next;

            public Node(TKey key, TValue value) { Key = key; Value = value; }
        }

        private readonly Node?[] _buckets;
        private readonly IEqualityComparer<TKey> _comparer;
        private const int DefaultCapacity = 16;

        public SimpleHashTable(int capacity = DefaultCapacity, IEqualityComparer<TKey>? comparer = null)
        {
            _buckets = new Node?[capacity];
            _comparer = comparer ?? EqualityComparer<TKey>.Default;
        }

        private int GetBucketIndex(TKey key)
        {
            int hash = _comparer.GetHashCode(key) & 0x7FFFFFFF;
            return hash % _buckets.Length;
        }

        public void AddOrUpdate(TKey key, TValue value)
        {
            int idx = GetBucketIndex(key);
            var current = _buckets[idx];
            while (current != null)
            {
                if (_comparer.Equals(current.Key, key))
                {
                    current.Value = value; // update existing
                    return;
                }
                current = current.Next;
            }
            var newNode = new Node(key, value) { Next = _buckets[idx] };
            _buckets[idx] = newNode;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            int idx = GetBucketIndex(key);
            var current = _buckets[idx];
            while (current != null)
            {
                if (_comparer.Equals(current.Key, key))
                {
                    value = current.Value;
                    return true;
                }
                current = current.Next;
            }
            value = default!;
            return false;
        }

        public bool ContainsKey(TKey key)
        {
            int idx = GetBucketIndex(key);
            var current = _buckets[idx];
            while (current != null)
            {
                if (_comparer.Equals(current.Key, key)) return true;
                current = current.Next;
            }
            return false;
        }

        public bool Remove(TKey key)
        {
            int idx = GetBucketIndex(key);
            Node? current = _buckets[idx];
            Node? prev = null;
            while (current != null)
            {
                if (_comparer.Equals(current.Key, key))
                {
                    if (prev == null) _buckets[idx] = current.Next;
                    else prev.Next = current.Next;
                    return true;
                }
                prev = current;
                current = current.Next;
            }
            return false;
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> Enumerate()
        {
            foreach (var bucket in _buckets)
            {
                var current = bucket;
                while (current != null)
                {
                    yield return new KeyValuePair<TKey, TValue>(current.Key, current.Value);
                    current = current.Next;
                }
            }
        }

        // NEW: Public accessor method to retrieve a value by key.
        public TValue Get(TKey key)
        {
            if (TryGetValue(key, out TValue value))
            {
                return value;
            }
            // If the key is not found, throw an exception similar to Dictionary<TKey, TValue>
            throw new KeyNotFoundException($"The given key '{key}' was not present in the hash table.");
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Enumerate().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
