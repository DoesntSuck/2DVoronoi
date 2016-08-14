using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Graph2D
{
    public static class GraphFactory
    {
        public static Graph DualGraph(Graph graph)
        {
            return null;
        }

        public static Graph DelaunayTriangulation(Vector2[] vectors, Vector2 origin, float radius)
        {
            Graph delaunay = new Graph();

            // Create super triangle
            GraphNode[] superTriangleNodes = GraphUtility.InsertSuperTriangle(delaunay, origin, radius);

            // Insert vectors one at a time...
            foreach (Vector2 vector in vectors)
            {
                // Add new node to graph
                GraphNode newNode = delaunay.AddNode(vector);

                // Find guilty triangles
                IEnumerable<GraphTriangle> guiltyTriangles = GraphUtility.WithinCircumcircles(delaunay.Triangles, vector);

                // Seperate triangles into inside and outside constituent edges
                HashSet<GraphEdge> insideEdges;
                HashSet<GraphEdge> outsideEdges;
                GraphUtility.InsideOutsideEdges(out insideEdges, out outsideEdges, guiltyTriangles);

                // Remove guilty triangles from graph
                foreach (GraphTriangle triangle in guiltyTriangles)
                    delaunay.Remove(triangle);

                // Remove inside edges from graph
                foreach (GraphEdge insideEdge in insideEdges)
                    delaunay.Remove(insideEdge);

                // Triangulate hole left by removed edges
                foreach (GraphEdge outsideEdge in outsideEdges)
                    delaunay.CreateTriangle(outsideEdge, newNode);
            }

            // Remove super triangle
            foreach (GraphNode node in superTriangleNodes)
                delaunay.Remove(node);

            return delaunay;
        }
    }
}
