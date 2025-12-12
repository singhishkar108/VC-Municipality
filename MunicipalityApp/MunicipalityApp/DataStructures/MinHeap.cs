using System;
using System.Collections.Generic;

namespace MunicipalityApp.DataStructures
{
    public class MinHeap<T>
    {
        private readonly List<T> _heap = new();
        private readonly Comparison<T> _comparison;

        public MinHeap(Comparison<T> comparison)
        {
            _comparison = comparison ?? throw new ArgumentNullException(nameof(comparison));
        }

        public int Count => _heap.Count;

        public void Insert(T item)
        {
            _heap.Add(item);
            SiftUp(_heap.Count - 1);
        }

        public T ExtractMin()
        {
            if (_heap.Count == 0) throw new InvalidOperationException("Heap is empty");
            var min = _heap[0];
            var last = _heap[_heap.Count - 1];
            _heap.RemoveAt(_heap.Count - 1);
            if (_heap.Count > 0)
            {
                _heap[0] = last;
                SiftDown(0);
            }
            return min;
        }

        public T Peek()
        {
            if (_heap.Count == 0) throw new InvalidOperationException("Heap is empty");
            return _heap[0];
        }

        private void SiftUp(int i)
        {
            var item = _heap[i];
            while (i > 0)
            {
                int parent = (i - 1) / 2;
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
                int left = 2 * i + 1;
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

        public bool Remove(T item)
        {
            int index = _heap.IndexOf(item);
            if (index == -1) return false;

            var last = _heap[_heap.Count - 1];
            _heap.RemoveAt(_heap.Count - 1);
            if (index < _heap.Count)
            {
                _heap[index] = last;
                SiftUp(index);
                SiftDown(index);
            }

            return true;
        }

        public void Clear() => _heap.Clear();
    }
}
