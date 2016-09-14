using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Graph2D
{
    public static class DelaunayTriangulation
    {
        public static float SuperTriangleScaleFactor = 1.5f;

        /// <summary>
        /// Creates a Delaunay Triangulation from the given vectors. A super triangle 
        /// </summary>
        public static Graph Create(Vector2[] vectors, bool removeSuperTriangle = false)
        {
            Graph triangulation = CreateSuperTriangleGraph(vectors);

            // Insert nodes at superTriangle vector positions
            GraphNode[] superTriangleNodes = new GraphNode[]
            {
                triangulation.Nodes[0],
                triangulation.Nodes[1],
                triangulation.Nodes[2]
            };

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

        public static void Insert(Graph triangulation, IEnumerable<Vector2> vectors)
        {
            // Insert each vector
            foreach (Vector2 vector in vectors)
                Insert(triangulation, vector);
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
        public static Graph CreateSuperTriangleGraph(Vector2[] vectors)
        {
            // Circle that encloses all vectors
            Circle incircle = MathExtension.BoundingCircle(vectors);

            // Direction from
            Vector2 topCentreDir = Vector2.up;
            Vector2 bottomLeftDir = Vector2.down + Vector2.left;
            Vector2 bottomRightDir = Vector2.down + Vector2.right;

            Vector2 topCentre = incircle.Centre + topCentreDir * ((float)incircle.Radius * 2) * SuperTriangleScaleFactor;
            Vector2 bottomLeft = incircle.Centre + bottomLeftDir * ((float)incircle.Radius * 2) * SuperTriangleScaleFactor;
            Vector2 bottomRight = incircle.Centre + bottomRightDir * ((float)incircle.Radius * 2) * SuperTriangleScaleFactor;

            Graph graph = new Graph();

            GraphNode a = graph.CreateNode(topCentre);
            GraphNode b = graph.CreateNode(bottomLeft);
            GraphNode c = graph.CreateNode(bottomRight);

            GraphEdge ab = graph.CreateEdge(a, b);
            GraphEdge ac = graph.CreateEdge(a, c);
            GraphEdge bc = graph.CreateEdge(b, c);

            graph.DefineTriangle(ab, ac, bc);

            return graph;
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
                    // Edge is 'inside' if it is shared by two triangles
                    // Edge is 'outside' if only one triangle knows about it

                    // If edge is in outside set (because another Triangle has already identified it), remove it and 
                    // add it to inside set because this triangle is now identifying it
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
