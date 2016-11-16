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

        public int id { get; private set; }

        /// <summary>
        /// An edge connecting the given nodes
        /// </summary>
        public GraphEdge(GraphNode node1, GraphNode node2)
        {
            Nodes = new GraphNode[] { node1, node2 };
            Triangles = new HashSet<GraphTriangle>();
            id = GetHashCode();
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

        public bool ContainsAny(IEnumerable<GraphNode> nodes)
        {
            foreach (GraphNode node in nodes)
                if (Contains(node)) return true;

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

        public int GetNodeIndex(GraphNode node)
        {
            for (int i = 0; i < Nodes.Length; i++)
                if (node == Nodes[i]) return i;

            return -1;
        }

        public void SetNodeAtIndex(GraphNode node, int index)
        {
            // Get each edge attached to CURRENT node
            foreach (GraphEdge edge in Nodes[index].Edges.Where(e => e != this))
            {
                // Change ref to the NEW node
                int nodeIndex = edge.GetNodeIndex(Nodes[index]);
                if (nodeIndex > -1) edge.Nodes[nodeIndex] = node;
            }

            // Get each triangle attached to CURRENT node
            foreach (GraphTriangle triangle in Nodes[index].Triangles)
            {
                // Change ref to the NEW node
                int nodeIndex = triangle.GetNodeIndex(Nodes[index]);
                if (nodeIndex > -1) triangle.Nodes[nodeIndex] = node;
            }

            // Update this edge's nodes
            Nodes[index] = node;
        }

        public override string ToString()
        {
            return "GraphEdge: " + 
                Nodes[0].Vector.ToString() + " -> " + 
                Nodes[1].Vector.ToString();
        }
    }
}
