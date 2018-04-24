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
            if (node1 == node2)
                throw new ArgumentException("Nodes must be distinct");

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
            return Nodes.Contains(node);
        }

        /// <summary>
        /// Checks if this edge is one of the edges that makes up the
        /// given triangle
        /// </summary>
        public bool IsConstituent(GraphTriangle triangle)
        {
            return Triangles.Contains(triangle);
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

        public override string ToString()
        {
            return "GraphEdge: " + 
                Nodes[0].Vector.ToString() + " -> " + 
                Nodes[1].Vector.ToString();
        }
    }
}
