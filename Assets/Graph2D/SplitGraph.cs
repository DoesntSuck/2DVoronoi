using System;
using System.Collections.Generic;
using System.Linq;

namespace Graph2D
{
    public class SplitGraph
    {
        public Graph Inside { get; private set; }
        public Graph Outside { get; private set; }

        /// <summary>
        /// Collection of nodes along the split edge. Nodes on the split edge are duplicated with one node occuring in each of the Inside and Outside 
        /// graphs. The 'Key' node is the node in the Outside graph, the 'Value' is the node in the Inside graph.
        /// </summary>
        public Dictionary<GraphNode, GraphNode> SplitNodes { get; set; }

        public SplitGraph(Graph outside, Graph inside)
        {
            Inside = inside;
            Outside = outside;
            SplitNodes = new Dictionary<GraphNode, GraphNode>();
        }

        public void AddSplitNode(GraphNode outside, GraphNode inside)
        {
            if (!SplitNodes.ContainsKey(outside))
                SplitNodes.Add(outside, inside);
        }

        public void Stitch()
        {
            // Get only splitNodes pairs that are present in BOTH graphs
            IEnumerable<KeyValuePair<GraphNode, GraphNode>> validNodes = SplitNodes.Where(s => Outside.Contains(s.Key) && Inside.Contains(s.Value));

            // Stitch outside graph to inside graph (so resultant graph is still 'Outside graph') using valid Nodes converted back to a dictionary
            Outside.Stitch(Inside, validNodes.ToDictionary(n => n.Key, n => n.Value));
        }
    }
}
