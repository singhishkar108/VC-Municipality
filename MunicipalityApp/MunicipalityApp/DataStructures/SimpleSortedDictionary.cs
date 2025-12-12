using System;
using System.Collections.Generic;

namespace MunicipalityApp.DataStructures
{
    public class SimpleSortedDictionary<TKey, TValue> where TKey : IComparable<TKey>
    {
        private readonly List<TKey> _keys = new();
        private readonly List<TValue> _values = new();

        public int Count => _keys.Count;

        private int IndexOf(TKey key)
        {
            int lo = 0, hi = _keys.Count - 1;
            while (lo <= hi)
            {
                int mid = lo + ((hi - lo) >> 1);
                int cmp = _keys[mid].CompareTo(key);
                if (cmp == 0) return mid;
                if (cmp < 0) lo = mid + 1; else hi = mid - 1;
            }
            return ~lo;
        }

        public void AddOrUpdate(TKey key, TValue value)
        {
            int idx = IndexOf(key);
            if (idx >= 0)
            {
                _values[idx] = value;
                return;
            }
            int ins = ~idx;
            _keys.Insert(ins, key);
            _values.Insert(ins, value);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            int idx = IndexOf(key);
            if (idx >= 0)
            {
                value = _values[idx];
                return true;
            }
            value = default!;
            return false;
        }

        public bool Remove(TKey key)
        {
            int idx = IndexOf(key);
            if (idx >= 0)
            {
                _keys.RemoveAt(idx);
                _values.RemoveAt(idx);
                return true;
            }
            return false;
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> Enumerate()
        {
            for (int i = 0; i < _keys.Count; i++)
                yield return new KeyValuePair<TKey, TValue>(_keys[i], _values[i]);
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> Range(TKey from, TKey to)
        {
            int start = IndexOf(from);
            if (start < 0) start = ~start;
            for (int i = start; i < _keys.Count; i++)
            {
                if (_keys[i].CompareTo(to) > 0) break;
                yield return new KeyValuePair<TKey, TValue>(_keys[i], _values[i]);
            }
        }
    }
}
