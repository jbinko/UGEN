// MIT License

// Copyright (c) 2020 Jiri Binko

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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
