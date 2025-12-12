// DataStructures/Graph.cs (MODIFIED)
using System;
using System.Collections.Generic;
using System.Linq; // Added for helper methods

namespace MunicipalityApp.DataStructures
{
    public class GraphNode<T>
    {
        public T Value;
        // Neighbors are explicitly the 'next' nodes (Directed)
        public List<GraphNode<T>> Neighbors;
        public GraphNode(T value)
        {
            Value = value;
            Neighbors = new List<GraphNode<T>>();
        }
    }

    public class Graph<T>
    {
        private readonly List<GraphNode<T>> _nodes = new();
        private readonly Dictionary<T, GraphNode<T>> _nodeMap = new();

        public GraphNode<T> AddNode(T value)
        {
            if (_nodeMap.ContainsKey(value)) return _nodeMap[value];
            var node = new GraphNode<T>(value);
            _nodes.Add(node);
            _nodeMap.Add(value, node);
            return node;
        }

        // MODIFIED: Only adds a directed edge from 'from' to 'to'
        public void AddDirectedEdge(T fromValue, T toValue)
        {
            var from = _nodeMap.GetValueOrDefault(fromValue);
            var to = _nodeMap.GetValueOrDefault(toValue);

            if (from == null || to == null) return;

            if (!from.Neighbors.Contains(to))
                from.Neighbors.Add(to);

            // REMOVED: if (!to.Neighbors.Contains(from)) to.Neighbors.Add(from);
        }

        public void BFS(GraphNode<T> start, Action<GraphNode<T>> visit)
        {
            var visited = new HashSet<GraphNode<T>>();
            var queue = new Queue<GraphNode<T>>();
            queue.Enqueue(start);
            visited.Add(start);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                visit(current);
                foreach (var neighbor in current.Neighbors)
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        public List<GraphNode<T>> Nodes => _nodes;
        // NEW: Helper method to retrieve a node by its value
        public GraphNode<T>? GetNode(T value) => _nodeMap.GetValueOrDefault(value);
    }
}