using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Graph2D;

namespace Graph2D
{
    public static class DelaunayTriangulation
    {
        public static Graph Create(Vector2[] superTriangle, Vector2[] vectors, bool removeSuperTriangle = false)
        {
            Graph triangulation = new Graph();

            // Insert nodes at superTriangle vector positions
            GraphNode[] superTriangleNodes = new GraphNode[]
            {
                triangulation.CreateNode(superTriangle[0]),
                triangulation.CreateNode(superTriangle[1]),
                triangulation.CreateNode(superTriangle[2])
            };

            // Add edges connecting nodes
            triangulation.CreateEdge(superTriangleNodes[0], superTriangleNodes[1]);
            triangulation.CreateEdge(superTriangleNodes[0], superTriangleNodes[2]);
            triangulation.CreateEdge(superTriangleNodes[1], superTriangleNodes[2]);

            // Convert edges to triangle
            triangulation.DefineTriangle(superTriangleNodes[0], superTriangleNodes[1], superTriangleNodes[2]);

            // Insert each vector into the triangulation maintaining its Delaunay-ness
            foreach (Vector2 vector in vectors)
                Insert(triangulation, vector);

            // Should the superTriangle be removed?
            if (removeSuperTriangle)
            {
                // Remove the superTriangle
                foreach (GraphNode superTriangleNode in superTriangleNodes)
                    triangulation.Destroy(superTriangleNode);
            }

            return triangulation;
        }

        /// <summary>
        /// Inserts the given vector as a node into the given graph. The graph is assumed to conform to the rules of a Delaunay triangulation.
        /// </summary>
        public static void Insert(Graph triangulation, Vector2 vector)
        {
            // Add new node to graph
            GraphNode newNode = triangulation.CreateNode(vector);

            // Find guilty triangles - triangles whose circumcircle contains the inserted vector
            List<GraphTriangle> guiltyTriangles = triangulation.Triangles.Where(t => t.Circumcircle.Contains(vector)).ToList();

            // Seperate triangles into inside and outside constituent edges
            HashSet<GraphEdge> insideEdges;
            HashSet<GraphEdge> outsideEdges;
            InsideOutsideEdges(out insideEdges, out outsideEdges, guiltyTriangles);

            // Remove guilty triangles from graph
            foreach (GraphTriangle triangle in guiltyTriangles)
                triangulation.Remove(triangle);

            // Remove inside edges from graph
            foreach (GraphEdge insideEdge in insideEdges)
                triangulation.Destroy(insideEdge);

            // Triangulate hole left by removed edges
            foreach (GraphEdge outsideEdge in outsideEdges)
                triangulation.CreateTriangle(outsideEdge, newNode);
        }

        public static void InsideOutsideEdges(out HashSet<GraphEdge> insideEdges, out HashSet<GraphEdge> outsideEdges, IEnumerable<GraphTriangle> triangles)
        {
            insideEdges = new HashSet<GraphEdge>();
            outsideEdges = new HashSet<GraphEdge>();

            // Get each edge from each triangle
            foreach (GraphTriangle triangle in triangles)
            {
                foreach (GraphEdge edge in triangle.Edges)
                {
                    // If edge is in outside set, remove it and add it to inside set
                    if (outsideEdges.Remove(edge))
                        insideEdges.Add(edge);

                    // Edge is not in outside set or inside set, add to outside set
                    else if (!insideEdges.Contains(edge))
                        outsideEdges.Add(edge);
                }
            }
        }
    }
}
