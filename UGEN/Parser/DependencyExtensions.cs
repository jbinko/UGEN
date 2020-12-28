using System;
using System.Linq;
using System.Collections.Generic;

namespace UGEN
{
    internal static class DependencyExtensions
    {
        private enum VisitState
        {
            NotVisited,
            Visiting,
            Visited
        };

        internal sealed class TopologySortResult<T>
        {
            public IEnumerable<T> Sorted { get; set; }
            public List<List<T>> Cycles { get; set; }
        }

        public static TopologySortResult<T> TopologySort<T>(this IEnumerable<T> nodes, Func<T, IEnumerable<T>> edges)
        {
            var cycles = new List<List<T>>();
            var visited = new Dictionary<T, VisitState>();
            foreach (var node in nodes)
                DepthFirstSearch(node, edges, new List<T>(), visited, cycles);
            return new TopologySortResult<T>
            {
                Sorted = visited.Keys.Reverse(),
                Cycles = cycles
            };
        }

        public static TopologySortResult<T> TopologySort<T, TValueList>(this IDictionary<T, TValueList> listDictionary)
            where TValueList : class, IEnumerable<T>
        {
            return listDictionary.Keys.TopologySort(key => listDictionary.ValueOrDefault(key, null) ?? Enumerable.Empty<T>());
        }

        private static void DepthFirstSearch<T>(T node, Func<T, IEnumerable<T>> lookup, List<T> parents,
            Dictionary<T, VisitState> visited, List<List<T>> cycles)
        {
            var state = visited.ValueOrDefault(node, VisitState.NotVisited);
            if (state == VisitState.Visited)
                return;
            else if (state == VisitState.Visiting)
                cycles.Add(parents.Concat(new[] { node }).SkipWhile(parent => !EqualityComparer<T>.Default.Equals(parent, node)).ToList());
            else
            {
                visited[node] = VisitState.Visiting;
                parents.Add(node);
                foreach (var child in lookup(node))
                    DepthFirstSearch(child, lookup, parents, visited, cycles);
                parents.RemoveAt(parents.Count - 1);
                visited[node] = VisitState.Visited;
            }
        }

        private static TValue ValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            TValue value;
            if (dictionary.TryGetValue(key, out value))
                return value;
            return defaultValue;
        }
    }
}
