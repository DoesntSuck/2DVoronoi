using System;
using System.Linq;
using UnityEngine;

namespace Graph2D
{
    /// <summary>
    /// Triangle object for containing three nodes that are joined by edges in a graph
    /// </summary>
    public class GraphTriangle
    {
        /// <summary>
        /// The three consituent nods of this triangle
        /// </summary>
        public GraphNode[] Nodes { get; private set; }

        /// <summary>
        /// The three constituent edges of this triangle
        /// </summary>
        public GraphEdge[] Edges { get; private set; }

        /// <summary>
        /// The circle that touches all three points of this triangle
        /// </summary>
        public Circle Circumcircle
        {
            get
            {
                // Lazy initialisation of circumcircle
                if (circumcircle == null)
                    circumcircle = MathExtension.Circumcircle(Nodes[0].Vector, Nodes[1].Vector, Nodes[2].Vector);

                return circumcircle;
            }
        }
        private Circle circumcircle;

        /// <summary>
        /// A triangle containing the three given nodes. Throws an error if the nodes are not connected by edges
        /// </summary>
        public GraphTriangle(GraphNode a, GraphNode b, GraphNode c)
        {
            Nodes = new GraphNode[] { a, b, c };
            Edges = new GraphEdge[Nodes.Length];

            // Find edges that connect nodes
            int edgeIndex = 0;
            for (int i = 0; i < Nodes.Length - 1; i++)
            {
                for (int j = i + 1; j < Nodes.Length; j++)
                {
                    // Add edge, connecting node i and j, to array
                    foreach (GraphEdge edge in Nodes[i].Edges.Where(e => e.Contains(Nodes[j])))
                        Edges[edgeIndex++] = edge;  // Index is incremented AFTER edge is added
                }
            }

            // Check that the given nodes connect to form a triangle
            foreach (GraphEdge edge in Edges)
            {
                if (edge == null)
                    throw new ArgumentException("Nodes are not connected and do not constitute a triangle");
            }
        }

        /// <summary>
        /// Checks if this triangle contains the given node
        /// </summary>
        public bool Contains(GraphNode node)
        {
            return Nodes.Contains(node);
        }

        public bool ContainsAny(params GraphNode[] nodes)
        {
            foreach (GraphNode node in nodes)
            {
                if (Contains(node))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Check if this triangle contains the given edge
        /// </summary>
        public bool Contains(GraphEdge edge)
        {
            foreach (GraphEdge myEdge in Edges)
            {
                if (myEdge.Equals(edge))
                    return true;
            }

            return false;
        }
    }
}
