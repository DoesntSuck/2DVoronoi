using System;
using System.Linq;
using System.Collections.Generic;
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
            get     // Lazy initialisation of circumcircle
            {
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
            Edges = new GraphEdge[] 
            {
                a.GetEdge(b),
                b.GetEdge(c),
                c.GetEdge(a)
            };

            // Check that there are edges connecting all the nodes
            if (Edges.Contains(null))
                throw new ArgumentException("Nodes are not connected and do not constitute a triangle");
        }

        /// <summary>
        /// Checks if this triangle contains the given node
        /// </summary>
        public bool Contains(GraphNode node)
        {
            return Nodes.Contains(node);
        }

        /// <summary>
        /// Checks if this triangle contains ANY of the given nodes
        /// </summary>
        public bool ContainsAny(params GraphNode[] nodes)
        {
            // Check each node
            foreach (GraphNode node in nodes)
            {
                if (Contains(node))
                    return true;
            }

            // None of the nodes were used by this triangle
            return false;
        }

        /// <summary>
        /// Check if this triangle contains the given edge
        /// </summary>
        public bool Contains(GraphEdge edge)
        {
            // Check each edge for the given edge
            foreach (GraphEdge myEdge in Edges)
            {
                if (myEdge.Equals(edge))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Checks whether this triangle and the given triangle share an edge
        /// </summary>
        public bool SharesEdge(GraphTriangle other)
        {
            // Compare each edge
            foreach (GraphEdge edge in Edges)
            {
                // Check if triangles share edge
                if (other.Contains(edge))
                    return true;
            }

            // No edges were shared
            return false;
        }

        /// <summary>
        /// Returns a list of indices of the nodes in this triangle that are 'inside' the given edge.
        /// </summary>
        public List<int> SameSideNodeIndices(GraphEdge clipEdge, float side)
        {
            // List to store the indices of tri-nodes that are inside the clip edge
            List<int> insideIndices = new List<int>();
            for (int i = 0; i < Nodes.Length; i++)
            {
                float nodeSide = MathExtension.Side(clipEdge.Nodes[0].Vector, clipEdge.Nodes[1].Vector, Nodes[i].Vector);

                // If this node is on the same side OR on the line
                if (nodeSide == side || nodeSide == 0)
                    insideIndices.Add(i);
            }

            return insideIndices;
        }
    }
}
