using System;
using System.Collections.Generic;

namespace MunicipalityApp.DataStructures
{
    public class SimplePriorityQueue<T>
    {
        private readonly List<T> _heap = new();
        private readonly Comparison<T> _comparison;

        public SimplePriorityQueue(Comparison<T> comparison)
        {
            _comparison = comparison ?? throw new ArgumentNullException(nameof(comparison));
        }

        public int Count => _heap.Count;

        public void Enqueue(T item)
        {
            _heap.Add(item);
            SiftUp(_heap.Count - 1);
        }

        public T Dequeue()
        {
            if (_heap.Count == 0) throw new InvalidOperationException("Queue is empty");
            var result = _heap[0];
            var last = _heap[_heap.Count - 1];
            _heap.RemoveAt(_heap.Count - 1);
            if (_heap.Count > 0)
            {
                _heap[0] = last;
                SiftDown(0);
            }
            return result;
        }

        public T Peek()
        {
            if (_heap.Count == 0) throw new InvalidOperationException("Queue is empty");
            return _heap[0];
        }

        private void SiftUp(int i)
        {
            var item = _heap[i];
            while (i > 0)
            {
                int parent = (i - 1) >> 1;
                if (_comparison(_heap[parent], item) <= 0) break;
                _heap[i] = _heap[parent];
                i = parent;
            }
            _heap[i] = item;
        }

        private void SiftDown(int i)
        {
            int count = _heap.Count;
            var item = _heap[i];
            while (true)
            {
                int left = (i << 1) + 1;
                if (left >= count) break;
                int right = left + 1;
                int smallest = left;
                if (right < count && _comparison(_heap[right], _heap[left]) < 0)
                    smallest = right;
                if (_comparison(item, _heap[smallest]) <= 0) break;
                _heap[i] = _heap[smallest];
                i = smallest;
            }
            _heap[i] = item;
        }

        public void Clear() => _heap.Clear();

        public void BuildFrom(IEnumerable<T> items)
        {
            _heap.Clear();
            _heap.AddRange(items);
            for (int i = (_heap.Count / 2) - 1; i >= 0; i--)
                SiftDown(i);
        }
    }
}
