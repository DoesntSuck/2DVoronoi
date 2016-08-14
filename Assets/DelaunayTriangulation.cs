using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Graph2D;

namespace Assets
{
    public class DelaunayTriangulation
    {
        /// <summary>
        /// The Graph data structure containing nodes, edges, and triangles
        /// </summary>
        public Graph Graph { get; private set; }
        private GraphNode[] superTriangleNodes;

        private DelaunayTriangulation(Vector2 origin, float radius)
        {
            Graph = new Graph();

            superTriangleNodes = GraphUtility.InsertSuperTriangle(Graph, origin, radius);
        }

        public DelaunayTriangulation Insert(Vector2 vector)
        {
            // Add new node to graph
            GraphNode newNode = Graph.AddNode(vector);

            // Find guilty triangles
            List<GraphTriangle> guiltyTriangles = GraphUtility.WithinCircumcircles(Graph.Triangles, vector).ToList();

            // Seperate triangles into inside and outside constituent edges
            HashSet<GraphEdge> insideEdges;
            HashSet<GraphEdge> outsideEdges;
            GraphUtility.InsideOutsideEdges(out insideEdges, out outsideEdges, guiltyTriangles);

            // Remove guilty triangles from graph
            foreach (GraphTriangle triangle in guiltyTriangles)
                Graph.Remove(triangle);

            // Remove inside edges from graph
            foreach (GraphEdge insideEdge in insideEdges)
                Graph.Remove(insideEdge);

            // Triangulate hole left by removed edges
            foreach (GraphEdge outsideEdge in outsideEdges)
                Graph.CreateTriangle(outsideEdge, newNode);

            // Builder pattern: return self
            return this;
        }

        public DelaunayTriangulation InsertRange(IEnumerable<Vector2> vectors)
        {
            // Insert each vector
            foreach (Vector2 vector in vectors)
                Insert(vector);

            return this;
        }
    }
}
