using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Graph2D
{
    public class DelaunayTriangulation
    {
        /// <summary>
        /// How far to extend the super triangle nodes from the origin
        /// </summary>
        private const float DEFAULT_SUPER_TRIANGLE_SCALE_FACTOR = 10;

        private static readonly Vector2[] DEFAULT_SUPER_TRIANGLE = new Vector2[] 
        {
            Vector2.up * DEFAULT_SUPER_TRIANGLE_SCALE_FACTOR,
            (Vector2.down + Vector2.left) * DEFAULT_SUPER_TRIANGLE_SCALE_FACTOR,
            (Vector2.down + Vector2.right) * DEFAULT_SUPER_TRIANGLE_SCALE_FACTOR
        };

        /// <summary>
        /// The nodes, edges, and triangles that constitute this triangulation
        /// </summary>
        public Graph Graph { get; private set; }

        /// <summary>
        /// Gets the nodes that make up the super triangle. The super triangle is a triangle large
        /// enough to contain all points in this triangulation
        /// </summary>
        public GraphNode[] SuperTriangle { get; private set; }

        /// <summary>
        /// Create a new Delaunay triangulation with an arbitrarily sized super triangle: (0,1), (-1, -1), (1, -1). The super 
        /// triangle is assumed to be large enough to contain all of the points that will be inserted into this triangulation.
        /// </summary>
        public DelaunayTriangulation()
        {
            AddSuperTriangle(DEFAULT_SUPER_TRIANGLE);
        }

        /// <summary>
        /// Create a new Delaunay triangulation with a super triangle that encapsulates the given bounds. The super triangle is assumed
        /// to be large enough to contain all of the points that will be inserted into this triangulation
        /// </summary>
        public DelaunayTriangulation(Circle contentBounds)
        {
            // Distance from incircle centre to the verts on the containing equilateral triangle
            float d = Geometry.DistanceFromIncircleCentreToEquilateralVertex(contentBounds);
            float adjustedD = d * 1.5f;

            // Direction from content bounds centre
            Vector2 topCentre = Vector2.up * adjustedD;
            Vector2 bottomLeft = (Vector2.down + Vector2.left) * adjustedD;
            Vector2 bottomRight = (Vector2.down + Vector2.right) * adjustedD;

            // Store in an array to pass to Initialize method
            Vector2[] superTriangle = new Vector2[] { topCentre, bottomLeft, bottomRight };
            AddSuperTriangle(superTriangle);
        }

        /// <summary>
        /// Create a new Delaunay triangulation with a super triangle defined by the given vectors. The super triangle is assumed
        /// to contain all of the points that will be inserted into this triangulation
        /// </summary>
        public DelaunayTriangulation(Vector2[] superTriangle)
        {
            AddSuperTriangle(superTriangle);
        }

        /// <summary>
        /// Insert all of the given vectors into this triangulation
        /// </summary>
        public void Insert(IEnumerable<Vector2> vectors)
        {
            // Insert each vector
            foreach (Vector2 vector in vectors)
                Insert(vector);
        }

        /// <summary>
        /// Inserts the given vector as a node into the given graph. The graph is assumed to conform to the rules of a Delaunay triangulation.
        /// </summary>
        public void Insert(Vector2 vector)
        {
            if (!Geometry.TriangleContains(vector, SuperTriangle[0].Vector, SuperTriangle[1].Vector, SuperTriangle[2].Vector))
                Debug.Log("Outside: " + vector);

            // Add new node to graph
            GraphNode newNode = Graph.CreateNode(vector);

            // Find guilty triangles - triangles whose circumcircle contains the inserted vector
            List<GraphTriangle> guiltyTriangles = Graph.Triangles.Where(t => t.Circumcircle.Contains(vector)).ToList();

            // Seperate triangles into inside and outside constituent edges
            HashSet<GraphEdge> insideEdges;
            HashSet<GraphEdge> outsideEdges;
            InsideOutsideEdges(guiltyTriangles, out insideEdges, out outsideEdges);

            // Remove guilty triangles from graph
            foreach (GraphTriangle triangle in guiltyTriangles)
                Graph.Remove(triangle);

            // Remove inside edges from graph
            foreach (GraphEdge insideEdge in insideEdges)
                Graph.Destroy(insideEdge);

            // Triangulate hole left by removed edges
            foreach (GraphEdge outsideEdge in outsideEdges)
                Graph.CreateTriangle(outsideEdge, newNode);
        }

        /// <summary>
        /// Initializes the triangulation with a new super triangle defined by the given vectors. The super triangle is assumed to
        /// encapsulate all of the points that will be inserted into this triangulation
        /// </summary>
        private void AddSuperTriangle(Vector2[] superTriangle)
        {
            // Need 3 nodes EXACTLY
            if (superTriangle.Length != 3)
                throw new ArgumentException("Vectors do not constitute a triangle");

            // New graph to hold triangulation
            Graph = new Graph();

            // Create a node for each super triangle vector
            GraphNode a = Graph.CreateNode(superTriangle[0]);
            GraphNode b = Graph.CreateNode(superTriangle[1]);
            GraphNode c = Graph.CreateNode(superTriangle[2]);

            // Remember which nodes constitute the super triangle
            SuperTriangle = new GraphNode[] { a, b, c };

            // Create edges between the super triangle nodes
            GraphEdge ab = Graph.CreateEdge(a, b);
            GraphEdge ac = Graph.CreateEdge(a, c);
            GraphEdge bc = Graph.CreateEdge(b, c);

            // Define a triangle from the edges
            Graph.DefineTriangle(ab, ac, bc);
        }

        /// <summary>
        /// Remove the super triangle from this triangulation. Triangules / edges reliant upon the super triangle's nodes will
        /// be removed also.
        /// </summary>
        public void RemoveSuperTriangle()
        {
            // Remove the superTriangle from triangulation
            foreach (GraphNode superTriangleNode in SuperTriangle)
                Graph.Destroy(superTriangleNode);

            // Remove ref to nodes
            SuperTriangle = null;
        }

        /// <summary>
        /// Sorts edges of the given triangles into two sets: one containing edges internal 
        /// to the collective polygon, and one containing edges on the outside of the 
        /// collective polygon
        /// </summary>
        private void InsideOutsideEdges(IEnumerable<GraphTriangle> triangles, out HashSet<GraphEdge> insideEdges, out HashSet<GraphEdge> outsideEdges)
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
