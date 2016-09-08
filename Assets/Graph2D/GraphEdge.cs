using System;
using System.Collections.Generic;
using System.Linq;

namespace Graph2D
{
    /// <summary>
    /// Edge object for connecting two nodes in a graph
    /// </summary>
    public class GraphEdge
    {
        /// <summary>
        /// Pair of nodes that this edge connects
        /// </summary>
        public GraphNode[] Nodes { get; private set; }

        /// <summary>
        /// Set of all triangles of which this edge is a constituent
        /// </summary>
        public HashSet<GraphTriangle> Triangles { get; private set; }

        /// <summary>
        /// An edge connecting the given nodes
        /// </summary>
        public GraphEdge(GraphNode node1, GraphNode node2)
        {
            Nodes = new GraphNode[] { node1, node2 };
            Triangles = new HashSet<GraphTriangle>();
        }

        /// <summary>
        /// Adds the given triangle to this edge's collection of triangles
        /// </summary>
        public void AddTriangle(GraphTriangle triangle)
        {
            Triangles.Add(triangle);
        }

        /// <summary>
        /// Removes the given triangle from this edge's collection of triangles
        /// </summary>
        public void RemoveTriangle(GraphTriangle triangle)
        {
            Triangles.Remove(triangle);
        }

        /// <summary>
        /// Checks if the given node is one of this edge's pair of nodes
        /// </summary>
        public bool Contains(GraphNode node)
        {
            // Check both nodes
            foreach (GraphNode myNode in Nodes)
            {
                // If the nodes contain same data they are equal
                if (myNode.Equals(node))
                    return true;
            }

            // Nethier of the pair of nodes are the given node
            return false;
        }

        /// <summary>
        /// Gets the opposing node to the given node
        /// </summary>
        public GraphNode GetOther(GraphNode node)
        {
            if (node.Equals(Nodes[0]))
                return Nodes[1];

            else if (node.Equals(Nodes[1]))
                return Nodes[0];

            // Node is not contained in this edge, throw an exception
            else throw new ArgumentException("Node is not part of this edge");
        }

        /// <summary>
        /// Checks if this edge's nodes and the given edge's nodes contain the same vector data
        /// </summary>
        public bool Equals(GraphEdge other)
        {
            // Check both nodes against this edge's nodes
            foreach (GraphNode node in other.Nodes)
            {
                // If one is not the same, then the edges are not the same
                if (!Contains(node))
                    return false;
            }

            return true;
        }
    }
}
