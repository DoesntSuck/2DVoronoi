using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Graph2D
{
    public static class DelaunayTriangulation
    {
        /// <summary>
        /// Creates a Delaunay Triangulation from the given vectors. A super triangle 
        /// </summary>
        public static Graph Create(Vector2[] vectors, Vector2[] superTriangle, bool removeSuperTriangle = false) // TODO: Change method signature (need to be able to mix and match optional arguments)
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

        /// <summary>
        /// Creates a new equilateral super triangle that encloses all of the given vectors. The distance from the triangles centre to each vertex is 3 times
        /// the radius of the triangles incircle.
        /// </summary>
        public static Vector2[] CreateSuperTriangle(Vector2[] vectors)
        {
            // Circle that encloses all vectors
            Circle incircle = MathExtension.BoundingCircle(vectors);

            // Create new equilateral super triangle
            Vector2[] superTriangle = new Vector2[]
            {
                    new Vector2(incircle.Centre.x,                                (float)(incircle.Centre.y + incircle.Radius * 3)),    // Top-Centre
                    new Vector2((float)(incircle.Centre.x - incircle.Radius * 3), (float)(incircle.Centre.y - incircle.Radius * 3)),    // Bottom-Left
                    new Vector2((float)(incircle.Centre.x + incircle.Radius * 3), (float)(incircle.Centre.y - incircle.Radius * 3))     // Bottom-Right
            };

            return superTriangle;
        }

        #region Private Methods

        /// <summary>
        /// Sorts edges of the given triangles into two sets: one containing edges internal to the collective polygon, and one containing edges on the
        /// outside of the collective polygon
        /// </summary>
        private static void InsideOutsideEdges(out HashSet<GraphEdge> insideEdges, out HashSet<GraphEdge> outsideEdges, IEnumerable<GraphTriangle> triangles)
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
        #endregion
    }
}
