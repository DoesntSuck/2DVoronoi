﻿using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

        /// <summary>
        /// A node containing the given vector
        /// </summary>
        public GraphNode(Vector2 vector)
        {
            Vector = vector;
            Edges = new List<GraphEdge>();
            Triangles = new HashSet<GraphTriangle>();
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

        /// <summary>
        /// Gets the edge that connects this node to the given node.
        /// Returns null if there is no edge connecting the two nodes.
        /// </summary>
        public GraphEdge GetEdge(GraphNode other)
        {
            return Edges.Where(e => e.Contains(other)).FirstOrDefault();
        }

        /// <summary>
        /// Checks if this node is connected by an edge to the given 
        /// node.
        /// </summary>
        public bool IsNeighbour(GraphNode node)
        {
            return Edges.SelectMany(e => e.Nodes).Contains(node);
        }
        
        public override string ToString()
        {
            return "GraphNode: " + Vector.ToString();
        }
    }
}
