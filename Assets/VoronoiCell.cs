using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Graph2D;
using UnityEngine;

namespace Assets
{
    public class VoronoiCell
    {
        public Vector2 Nuclei { get; private set; }
        public HashSet<GraphNode> Nodes { get; private set; }
        public HashSet<GraphEdge> Edges { get; private set; }

        public VoronoiCell(Vector2 nuclei)
        {
            Nuclei = nuclei;
            Nodes = new HashSet<GraphNode>();
            Edges = new HashSet<GraphEdge>();
        }

        public void AddNode(GraphNode node)
        {
            Nodes.Add(node);

            // Check for edges bordering this cell
            foreach (GraphEdge edge in node.Edges)
            {
                // If cell already knows about this edges' other node, add the edge to cells border edges
                if (Nodes.Contains(edge.GetOther(node)))
                    Edges.Add(edge);
            }
        }

        public void Remove(GraphNode node)
        {
            // Remove node ref
            Nodes.Remove(node);

            // Remove refs to node's edges
            foreach (GraphEdge edge in node.Edges)
                Edges.Remove(edge);
        }

        /// <summary>
        /// Checks if the edges of this voronoi cell form a closed polygon
        /// </summary>
        public bool Closed()
        {
            // Track the nodes we have visited
            HashSet<GraphNode> visitedNodes = new HashSet<GraphNode>();
            foreach (GraphEdge edge in Edges)
            {
                // If false, node already exists in set, if both are false, we are connecting to a node we have already visited
                int nodesPresent = 0;
                if (!visitedNodes.Add(edge.Nodes[0])) nodesPresent++;
                if (!visitedNodes.Add(edge.Nodes[1])) nodesPresent++;

                // If connecting to a node we have already visited, then the polygon is closed
                if (nodesPresent == 2)
                    return true;
            }

            // Didn't close the polygon
            return false;
        }

        public bool Contains(GraphNode node)
        {
            return Nodes.Contains(node);
        }

        public bool Contains(GraphEdge edge)
        {
            return Edges.Contains(edge);
        }
    }
}
