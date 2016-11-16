using System;
using System.Collections.Generic;
using UnityEngine;

namespace Graph2D
{
    /// <summary>
    /// Node object for storing a vector
    /// </summary>
    public class GraphNode
    {
         /// <summary>
        /// This nodes vector data
        /// </summary>
        public Vector2 Vector { get; set; }

        /// <summary>
        /// Set of all edges of which this node is a consituent
        /// </summary>
        public List<GraphEdge> Edges { get; private set; }

        /// <summary>
        /// et of all triangles of which this node is a consituent
        /// </summary>
        public HashSet<GraphTriangle> Triangles { get; private set; }

        public int id { get; private set; }

        /// <summary>
        /// A node containing the given vector
        /// </summary>
        public GraphNode(Vector2 vector)
        {
            Vector = vector;
            Edges = new List<GraphEdge>();
            Triangles = new HashSet<GraphTriangle>();
            id = GetHashCode();
        }

        /// <summary>
        /// Adds the given edge to this node's collection of edges
        /// </summary>
        public void AddEdge(GraphEdge edge)
        {
            Edges.Add(edge);
        }

        /// <summary>
        /// Removes the given edge from this node's collection of edges
        /// </summary>
        public void RemoveEdge(GraphEdge edge)
        {
            Edges.Remove(edge);
        }

        /// <summary>
        /// Adds the given triangle to this node's collection of triangles
        /// </summary>
        public void AddTriangle(GraphTriangle triangle)
        {
            Triangles.Add(triangle);
        }

        /// <summary>
        /// Removes the given triangle from this node's collection of triangles
        /// </summary>
        public void RemoveTriangle(GraphTriangle triangle)
        {
            Triangles.Remove(triangle);
        }

        public GraphEdge GetEdge(GraphNode other)
        {
            foreach (GraphEdge edge in Edges)
            {
                if (edge.Contains(other))
                    return edge;
            }

            return null;
        }

        public bool HasEdge(GraphNode node)
        {
            // Check if ANY edge contains the given node
            foreach (GraphEdge edge in Edges)
            {
                if (edge.Contains(node))
                    return true;
            }

            // No edges contain the given node
            return false;
        }

        public void Switch(GraphNode replacement)
        {
            foreach (GraphEdge edge in Edges)
            {
                int index = edge.GetNodeIndex(this);
                edge.Nodes[index] = replacement;
                replacement.Edges.Add(edge);
            }

            foreach (GraphTriangle triangle in Triangles)
            {
                int index = triangle.GetNodeIndex(this);
                triangle.Nodes[index] = replacement;
                replacement.Triangles.Add(triangle);
            }
        }

        public override string ToString()
        {
            return "[" + Vector.x + ", " + Vector.y + "]";
        }
    }
}
