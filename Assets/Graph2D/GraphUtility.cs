using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Graph2D
{
    public static class GraphUtility
    {
        public static GraphNode[] InsertSuperTriangle(Graph graph, Vector2 origin, float distance)
        {
            // Add super triangle: triangle large enough to encompass all insertion vectors
            GraphNode[] superTriangleNodes = new GraphNode[]
            {
                graph.AddNode(new Vector2(origin.x,            origin.y + distance)),
                graph.AddNode(new Vector2(origin.x - distance, origin.y - distance)),
                graph.AddNode(new Vector2(origin.x + distance, origin.y - distance)),
            };

            // Add edges
            graph.AddEdge(superTriangleNodes[0], superTriangleNodes[1]);
            graph.AddEdge(superTriangleNodes[0], superTriangleNodes[2]);
            graph.AddEdge(superTriangleNodes[1], superTriangleNodes[2]);

            // Add triangle
            graph.AddTriangle(superTriangleNodes[0], superTriangleNodes[1], superTriangleNodes[2]);

            return superTriangleNodes;
        }

        public static IEnumerable<GraphTriangle> WithinCircumcircles(IEnumerable<GraphTriangle> triangles, Vector2 vector)
        {
            // Get all triangles where the triangles circumcircle contains the given vector
            return triangles.Where(t => t.Circumcircle.Contains(vector));
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
