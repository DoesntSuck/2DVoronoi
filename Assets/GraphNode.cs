using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets
{
    public class GraphNode
    {
        public Vector2 Vector { get; private set; }
        public HashSet<GraphEdge> Edges { get; private set; }
        public HashSet<GraphTriangle> Triangles { get; private set; }

        public GraphNode(Vector2 vector)
        {
            Vector = vector;
            Edges = new HashSet<GraphEdge>();
            Triangles = new HashSet<GraphTriangle>();
        }

        public void AddEdge(GraphEdge edge)
        {
            Edges.Add(edge);
        }

        public void RemoveEdge(GraphEdge edge)
        {
            Edges.Remove(edge);
        }

        public void AddTriangle(GraphTriangle triangle)
        {
            Triangles.Add(triangle);
        }

        public void RemoveTriangle(GraphTriangle triangle)
        {
            Triangles.Remove(triangle);
        }

        public bool Contains(GraphEdge edge)
        {
            return Edges.Contains(edge);
        }

        public bool Contains(GraphTriangle triangle)
        {
            return Triangles.Contains(triangle);
        }

        public bool Equals(GraphNode other)
        {
            return Vector == other.Vector;
        }
    }
}
