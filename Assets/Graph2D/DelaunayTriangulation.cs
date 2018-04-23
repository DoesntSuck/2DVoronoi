using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Graph2D
{
    public static class DelaunayTriangulation
    {
        /// <summary>
        /// The nodes, edges, and triangles that constitute this triangulation
        /// </summary>
        private static Graph graph;

        /// <summary>
        /// The nodes that make up the super triangle. The super triangle is a triangle large
        /// enough to contain all points in this triangulation
        /// </summary>
        private static GraphNode[] superTriangleNodes;

        /// <summary>
        /// Assembles the given points into a tri-graph structure where the triangles
        /// conform to the rules for a Delaunay triangulation.
        /// </summary>
        public static Graph Create(IEnumerable<Vector2> points, bool keepSuperTriangle = false)
        {
            // TODO: calculate super triangle that bounds all points
            Circle boundingCircle = Geometry.BoundingCircle(points);

            return Create(points, boundingCircle, keepSuperTriangle);
        }

        /// <summary>
        /// Assembles the given points into a tri-graph structure where the triangles
        /// conform to the rules for a Delaunay triangulation. The given triangle is
        /// assumed to contain all the given points.
        /// </summary>
        public static Graph Create(IEnumerable<Vector2> points, Vector2[] superTriangle, bool keepSuperTriangle = false)
        {
            AddSuperTriangle(superTriangle);
            Insert(points);

            if (!keepSuperTriangle)
                RemoveSuperTriangle();

            return graph;
        }

        /// <summary>
        /// Assembles the given points into a tri-graph structure where the triangles
        /// conform to the rules for a Delaunay triangulation. The given circle is
        /// assumed to bound all the given points.
        /// </summary>
        public static Graph Create(IEnumerable<Vector2> points, Circle bounds, bool keepSuperTriangle = false)
        {
            // Distance from incircle centre to the verts on the containing equilateral triangle
            float d = Geometry.DistanceFromIncircleCentreToEquilateralVertex(bounds);
            float adjustedD = d * 1.5f;

            // Direction from content bounds centre
            Vector2 topCentre = Vector2.up * adjustedD;
            Vector2 bottomLeft = (Vector2.down + Vector2.left) * adjustedD;
            Vector2 bottomRight = (Vector2.down + Vector2.right) * adjustedD;

            // Pass super triangle to overload
            Vector2[] superTriangle = new Vector2[] { topCentre, bottomLeft, bottomRight };
            return Create(points, superTriangle, keepSuperTriangle);
        }

        /// <summary>
        /// Insert all of the given vectors into this triangulation
        /// </summary>
        private static void Insert(IEnumerable<Vector2> vectors)
        {
            // Insert each vector
            foreach (Vector2 vector in vectors)
                Insert(vector);
        }

        /// <summary>
        /// Inserts the given vector as a node into the given graph. The graph is assumed to conform to the rules of a Delaunay triangulation.
        /// </summary>
        private static  void Insert(Vector2 vector)
        {
            if (!Geometry.TriangleContains(vector, superTriangleNodes[0].Vector, superTriangleNodes[1].Vector, superTriangleNodes[2].Vector))
                Debug.Log("Outside: " + vector);

            // Add new node to graph
            GraphNode newNode = graph.CreateNode(vector);

            // Find guilty triangles - triangles whose circumcircle contains the inserted vector
            List<GraphTriangle> guiltyTriangles = graph.Triangles.Where(t => t.Circumcircle.Contains(vector)).ToList();

            // Seperate triangles into inside and outside constituent edges
            HashSet<GraphEdge> insideEdges;
            HashSet<GraphEdge> outsideEdges;
            InsideOutsideEdges(guiltyTriangles, out insideEdges, out outsideEdges);

            // Remove guilty triangles from graph
            foreach (GraphTriangle triangle in guiltyTriangles)
                graph.Remove(triangle);

            // Remove inside edges from graph
            foreach (GraphEdge insideEdge in insideEdges)
                graph.Destroy(insideEdge);

            // Triangulate hole left by removed edges
            foreach (GraphEdge outsideEdge in outsideEdges)
                graph.CreateTriangle(outsideEdge, newNode);
        }

        /// <summary>
        /// Initializes the triangulation with a new super triangle defined by the given vectors. The super triangle is assumed to
        /// encapsulate all of the points that will be inserted into this triangulation
        /// </summary>
        private static void AddSuperTriangle(Vector2[] superTriangle)
        {
            // Need 3 nodes EXACTLY
            if (superTriangle.Length != 3)
                throw new ArgumentException("Vectors do not constitute a triangle");

            // New graph to hold triangulation
            graph = new Graph();

            // Create a node for each super triangle vector
            GraphNode a = graph.CreateNode(superTriangle[0]);
            GraphNode b = graph.CreateNode(superTriangle[1]);
            GraphNode c = graph.CreateNode(superTriangle[2]);

            // Remember which nodes constitute the super triangle
            superTriangleNodes = new GraphNode[] { a, b, c };

            // Create edges between the super triangle nodes
            GraphEdge ab = graph.CreateEdge(a, b);
            GraphEdge ac = graph.CreateEdge(a, c);
            GraphEdge bc = graph.CreateEdge(b, c);

            // Define a triangle from the edges
            graph.DefineTriangle(ab, ac, bc);
        }

        /// <summary>
        /// Remove the super triangle from this triangulation. Triangules / edges reliant upon the super triangle's nodes will
        /// be removed also.
        /// </summary>
        private static void RemoveSuperTriangle()
        {
            // Remove the superTriangle from triangulation
            foreach (GraphNode superTriangleNode in superTriangleNodes)
                graph.Destroy(superTriangleNode);
        }

        /// <summary>
        /// Sorts edges of the given triangles into two sets: one containing edges internal 
        /// to the collective polygon, and one containing edges on the outside of the 
        /// collective polygon
        /// </summary>
        private static void InsideOutsideEdges(IEnumerable<GraphTriangle> triangles, out HashSet<GraphEdge> insideEdges, out HashSet<GraphEdge> outsideEdges)
        {
            insideEdges = new HashSet<GraphEdge>();
            outsideEdges = new HashSet<GraphEdge>();

            // Get each edge from each triangle
            foreach (GraphEdge edge in triangles.SelectMany(t => t.Edges))
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
}
