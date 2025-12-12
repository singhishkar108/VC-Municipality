using System;
using System.Collections;
using System.Collections.Generic;

namespace MunicipalityApp.DataStructures
{
    // Fully custom hash set using separate chaining for collisions
    public class SimpleHashSet<T> : IEnumerable<T>
    {
        private class Node
        {
            public T Value;
            public Node? Next;
            public Node(T value) { Value = value; }
        }

        private readonly Node?[] _buckets;
        private readonly IEqualityComparer<T> _comparer;
        private const int DefaultCapacity = 16;

        public SimpleHashSet(int capacity = DefaultCapacity, IEqualityComparer<T>? comparer = null)
        {
            _buckets = new Node?[capacity];
            _comparer = comparer ?? EqualityComparer<T>.Default;
        }

        private int GetBucketIndex(T item)
        {
            int hash = _comparer.GetHashCode(item) & 0x7FFFFFFF;
            return hash % _buckets.Length;
        }

        public bool Add(T item)
        {
            int idx = GetBucketIndex(item);
            var current = _buckets[idx];
            while (current != null)
            {
                if (_comparer.Equals(current.Value, item)) return false; // already exists
                current = current.Next;
            }
            var newNode = new Node(item) { Next = _buckets[idx] };
            _buckets[idx] = newNode;
            return true;
        }

        public bool Remove(T item)
        {
            int idx = GetBucketIndex(item);
            Node? current = _buckets[idx];
            Node? prev = null;

            while (current != null)
            {
                if (_comparer.Equals(current.Value, item))
                {
                    if (prev == null)
                        _buckets[idx] = current.Next;
                    else
                        prev.Next = current.Next;
                    return true;
                }
                prev = current;
                current = current.Next;
            }
            return false;
        }

        public bool Contains(T item)
        {
            int idx = GetBucketIndex(item);
            var current = _buckets[idx];
            while (current != null)
            {
                if (_comparer.Equals(current.Value, item)) return true;
                current = current.Next;
            }
            return false;
        }

        public void Clear()
        {
            for (int i = 0; i < _buckets.Length; i++)
                _buckets[i] = null;
        }

        public List<T> ToList()
        {
            var list = new List<T>();
            foreach (var bucket in _buckets)
            {
                var current = bucket;
                while (current != null)
                {
                    list.Add(current.Value);
                    current = current.Next;
                }
            }
            return list;
        }

        public IEnumerator<T> GetEnumerator() => ToList().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
