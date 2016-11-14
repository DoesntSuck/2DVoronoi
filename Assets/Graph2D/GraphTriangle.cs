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
            get     // Lazy initialisation of circumcircle
            {
                if (circumcircle == null)
                    circumcircle = MathExtension.Circumcircle(Nodes[0].Vector, Nodes[1].Vector, Nodes[2].Vector);
                return circumcircle;
            }
        }
        private Circle circumcircle;

        public Circle Incircle
        {
            get
            {
                if (incircle == null)
                    incircle = MathExtension.Incircle(Nodes[0].Vector, Nodes[1].Vector, Nodes[2].Vector);
                return incircle;
            }
        }
        private Circle incircle;

        /// <summary>
        /// A triangle containing the three given nodes. Throws an error if the nodes are not connected by edges
        /// </summary>
        public GraphTriangle(GraphEdge a, GraphEdge b, GraphEdge c)
        {
            Edges = new GraphEdge[] { a, b, c };
            Nodes = a.Nodes.Union(b.Nodes).Union(c.Nodes).Distinct().ToArray();    // Get each unique node entry, convert to array

            // Check the triangle is valid
            Validate();
        }
        
        public void OrderNodes()
        {
            ClockwiseNodeComparer nodeComparer = new ClockwiseNodeComparer(Incircle.Centre);
            Array.Sort(Nodes, nodeComparer);
        }

        /// <summary>
        /// Checks if this triangle contains the given node
        /// </summary>
        public bool Contains(GraphNode node)
        {
            return Nodes.Contains(node);
        }

        /// <summary>
        /// Checks that this triangle contains ALL of the given nodes
        /// </summary>
        public bool Contains(params GraphNode[] nodes)
        {
            foreach (GraphNode node in nodes)
            {
                if (!Contains(node))
                    return false;
            }

            return true;
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
        /// Checks whether this triangle and the given triangle share a node
        /// </summary>
        public bool SharesNode(GraphTriangle other)
        {
            // Compare each edge
            foreach (GraphNode node in Nodes)
            {
                // Check if triangles share edge
                if (other.Contains(node))
                    return true;
            }

            // No edges were shared
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

        public override string ToString()
        {
            return "GraphTriangle: " + 
                Nodes[0].Vector.ToString() + " -> " + 
                Nodes[1].Vector.ToString() + " -> " + 
                Nodes[2].Vector.ToString();
        }

        private void Validate()
        {
            // Null check
            if (Edges.Contains(null) || Nodes.Contains(null))
                throw new ArgumentException("Triangle contains null node(s) or edge(s)");

            // Count check
            if (Nodes.Length != 3 || Edges.Length != 3)
                throw new ArgumentException("Triangle doesn't contain 3 nodes or 3 edges. Node count: " + Nodes.Length + ", Edge count: " + Edges.Length);

            for (int i = 0; i < Nodes.Length - 1; i++)
            {
                for (int j = i + 1; j < Nodes.Length; j++)
                {
                    // Duplicate check
                    if (Nodes[i] == Nodes[j])
                        throw new ArgumentException("Triangle contains duplicate nodes");

                    // Connecting edge check
                    if (!Nodes[i].HasEdge(Nodes[j]) || 
                        !Nodes[j].HasEdge(Nodes[i]))
                        throw new ArgumentException("Not all nodes in triangle are connected");
                }
            }

            /* At this point, the triangle has passed all validity tests! */
        }
    }
}
