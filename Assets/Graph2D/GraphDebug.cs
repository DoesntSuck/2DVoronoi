using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Graph2D
{
    public static class GraphDebug
    {
        public static Color NodeColour = Color.white;
        public static float NodeRadius = 0.025f;
        public static Color TriangleColour = Color.white;
        public static Color EdgeColour = Color.red;
        public static Color CircumcircleColour = Color.yellow;
        public static bool Circumcircles = false;

        public static void DrawGraph(Graph graph)
        {
            DrawNodes(graph.Nodes);
            DrawEdges(graph.Edges);
            DrawTriangles(graph.Triangles);

            if (Circumcircles)
                DrawCircumcircles(graph.Triangles);
        }

        public static void DrawVector(Vector3 position, Color colour, float radius)
        {
            // Remember original color, set new color
            Color original = Gizmos.color;
            Gizmos.color = colour;

            // Draw nodes
            Gizmos.DrawSphere(position, radius);

            // Reset color to original
            Gizmos.color = original;
        }

        public static void DrawVectors(IEnumerable<Vector3> positions, Color colour, float radius)
        {
            // Draw nodes
            foreach (Vector3 position in positions)
                DrawVector(position, colour, radius);
        }

        public static void DrawNodes(IEnumerable<GraphNode> nodes)
        {
            DrawVectors(nodes.Select(n => (Vector3)n.Vector), NodeColour, NodeRadius);
        }

        public static void DrawEdges(IEnumerable<GraphEdge> edges)
        {
            // Remember original color, set new color
            Color original = Gizmos.color;
            Gizmos.color = EdgeColour;

            // Draw line between each node
            foreach (GraphEdge edge in edges)
                Gizmos.DrawLine(edge.Nodes[0].Vector, edge.Nodes[1].Vector);

            // Reset color to original
            Gizmos.color = original;
        }

        public static void DrawTriangles(IEnumerable<GraphTriangle> triangles)
        {
            foreach (GraphTriangle triangle in triangles)
                DrawTriangle(triangle, TriangleColour);
        }

        public static void DrawTriangle(GraphTriangle triangle, Color colour)
        {
            // Remember original color, set new color
            Color original = Gizmos.color;
            Gizmos.color = colour;

            // Draw line between each node
            for (int i = 0; i < triangle.Nodes.Length - 1; i++)
            {
                for (int j = i + 1; j < triangle.Nodes.Length; j++)
                {
                    Gizmos.DrawLine(triangle.Nodes[i].Vector, triangle.Nodes[j].Vector);
                }
            }

            // Reset color to original
            Gizmos.color = original;
        }

        public static void DrawCircumcircles(IEnumerable<GraphTriangle> triangles)
        {
            foreach (GraphTriangle triangle in triangles)
                DrawCircumcircle(triangle, CircumcircleColour);
        }

        public static void DrawCircumcircle(GraphTriangle triangle, Color colour)
        {
            // Remember original color, set new color
            Color original = UnityEditor.Handles.color;
            Handles.color = colour;

            // Draw circumcircle for each triangle
            Handles.DrawWireDisc(triangle.Circumcircle.Centre, -Vector3.forward, (float)triangle.Circumcircle.Radius);
            DrawVector(triangle.Circumcircle.Centre, colour, NodeRadius);

            // Reset color to original
            Handles.color = original;
        }
    }
}
