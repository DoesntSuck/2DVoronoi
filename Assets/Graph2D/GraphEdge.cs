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
        /// Iterates through all edges that this edges' nodes are connected to
        /// </summary>
        public IEnumerable<GraphEdge> Edges
        {
            get
            {
                // Do for both nodes
                foreach (GraphNode node in Nodes)
                {
                    // Iterate through each nodes collection of edges (excluding this node)
                    foreach (GraphEdge edge in node.Edges.Where(e => e != this))
                        yield return edge;      // return each edge
                }
            }
        }

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
        /// Checks if the given nods are the same as this edge's pair of nodes
        /// </summary>
        public bool Contains(GraphNode a, GraphNode b)
        {
            return Contains(a) && Contains(b);
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
