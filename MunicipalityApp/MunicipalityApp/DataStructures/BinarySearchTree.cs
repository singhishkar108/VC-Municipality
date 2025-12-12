using System;
using System.Collections.Generic;

namespace MunicipalityApp.DataStructures
{
    public class BSTNode<TKey, TValue> where TKey : IComparable<TKey>
    {
        public TKey Key;
        public TValue Value;
        public BSTNode<TKey, TValue>? Left;
        public BSTNode<TKey, TValue>? Right;

        public BSTNode(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }

    public class BinarySearchTree<TKey, TValue> where TKey : IComparable<TKey>
    {
        private BSTNode<TKey, TValue>? _root;

        public void Insert(TKey key, TValue value)
        {
            _root = Insert(_root, key, value);
        }

        private BSTNode<TKey, TValue> Insert(BSTNode<TKey, TValue>? node, TKey key, TValue value)
        {
            if (node == null) return new BSTNode<TKey, TValue>(key, value);
            int cmp = key.CompareTo(node.Key);
            if (cmp < 0) node.Left = Insert(node.Left, key, value);
            else if (cmp > 0) node.Right = Insert(node.Right, key, value);
            else node.Value = value; // update existing
            return node;
        }

        public TValue? Find(TKey key)
        {
            var node = _root;
            while (node != null)
            {
                int cmp = key.CompareTo(node.Key);
                if (cmp == 0) return node.Value;
                node = cmp < 0 ? node.Left : node.Right;
            }
            return default;
        }

        public bool Remove(TKey key)
        {
            bool removed = false;
            _root = Remove(_root, key, ref removed);
            return removed;
        }

        private BSTNode<TKey, TValue>? Remove(BSTNode<TKey, TValue>? node, TKey key, ref bool removed)
        {
            if (node == null) return null;
            int cmp = key.CompareTo(node.Key);
            if (cmp < 0) node.Left = Remove(node.Left, key, ref removed);
            else if (cmp > 0) node.Right = Remove(node.Right, key, ref removed);
            else
            {
                removed = true;
                if (node.Left == null) return node.Right;
                if (node.Right == null) return node.Left;
                var min = Min(node.Right);
                node.Key = min.Key;
                node.Value = min.Value;
                node.Right = Remove(node.Right, min.Key, ref removed);
            }
            return node;
        }

        private BSTNode<TKey, TValue> Min(BSTNode<TKey, TValue> node)
        {
            while (node.Left != null) node = node.Left;
            return node;
        }

        public void Update(TKey key, TValue value)
        {
            var existing = Find(key);
            if (existing != null)
                Insert(key, value);
        }

        public void Clear()
        {
            _root = null;
        }

        public IEnumerable<TValue> InOrderTraversal()
        {
            var list = new List<TValue>();
            InOrderTraversal(_root, list);
            return list;
        }

        private void InOrderTraversal(BSTNode<TKey, TValue>? node, List<TValue> list)
        {
            if (node == null) return;
            InOrderTraversal(node.Left, list);
            list.Add(node.Value);
            InOrderTraversal(node.Right, list);
        }
    }
}
