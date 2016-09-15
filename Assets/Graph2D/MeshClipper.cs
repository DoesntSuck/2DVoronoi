using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Graph2D
{
    public static class MeshClipper
    {
        public static Mesh Clip(Mesh mesh, Graph convexClipShape)
        {
            // Create graph that mirrors mesh
            Graph meshGraph = new Graph(mesh);

            // Calculate polygonal centre of convex mesh
            Vector2 polygonalCentre = MathExtension.PolygonCentre(convexClipShape.Nodes.Select(n => n.Vector).ToList());

            // Clip once per graph edge
            foreach (GraphEdge clipEdge in convexClipShape.Edges)
            {
                // Which side of edge is counted as being inside?
                float inside = MathExtension.Side(clipEdge.Nodes[0].Vector, clipEdge.Nodes[1].Vector, polygonalCentre);

                // Clip edges that aren't inside of line
                meshGraph.Clip(clipEdge.Nodes[0].Vector, clipEdge.Nodes[1].Vector, inside);
            }

            // Return clipped mesh
            return meshGraph.ToMesh("ClippedMesh");
        }
    }
}
