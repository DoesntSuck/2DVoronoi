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
        /// Gets the maximum circle confined to all three edges of the triangle
        /// </summary>
        public Circle Incircle
        {
            get
            {
                // Lazy initialisation of incircle
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

            // Check that there are edges connecting all the nodes
            if (Edges.Contains(null) || 
                Nodes.Contains(null) || 
                Nodes.Length != 3 || 
                Nodes[0].Equals(Nodes[1]) || 
                Nodes[1].Equals(Nodes[2]) || 
                Nodes[0].Equals(Nodes[2]))
                throw new ArgumentException("Nodes and edges do not constitute a triangle");
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

        public IEnumerable<GraphNode> SameSideNodes(Vector2 edgePoint1, Vector2 edgePoint2, float side)
        {
            return Nodes.Where(n => MathExtension.Side(edgePoint1, edgePoint2, n.Vector) == side);
        }

        public IEnumerable<GraphNode> OpposideSideNodes(Vector2 edgePoint1, Vector2 edgePoint2, float side)
        {
            return Nodes.Where(n => MathExtension.Side(edgePoint1, edgePoint2, n.Vector) == -side);
        }

        public IEnumerable<GraphNode> OnEdgeNodes(Vector2 edgePoint1, Vector2 edgePoint2)
        {
            return Nodes.Where(n => MathExtension.Side(edgePoint1, edgePoint2, n.Vector) == 0);
        }

        public override string ToString()
        {
            return "GraphTriangle: " + 
                Nodes[0].Vector.ToString() + " -> " + 
                Nodes[1].Vector.ToString() + " -> " + 
                Nodes[2].Vector.ToString();
        }
    }
}
