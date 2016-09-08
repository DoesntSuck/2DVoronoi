//using UnityEngine;
//using System.Collections;

//[RequireComponent(typeof(GraphFilter))]
//public class GraphGizmoRenderer : MonoBehaviour
//{
//    private GraphFilter graphFilter;

//    void OnDrawGizmos()
//    {
//        // Draw delaunay graph
//        if (delaunay != null)
//        {
//            // Draw circumcircles
//            if (DrawCircumcircle)   // Draw only circumcircles not relating to the super triangle
//                DrawCircumcircles(delaunay.Triangles.Where(t => !t.ContainsAny(superTriangleNodes)), CircumcircleColor);

//            // Draw edges
//            DrawEdges(delaunay.Edges, EdgeColor);

//            // Draw triangles
//            DrawTriangles(delaunay.Triangles, TriangleColor);

//            // Draw outside edges
//            if (outsideEdges != null)
//                DrawEdges(outsideEdges, OutsideEdgesColor);

//            // Draw inside edges
//            if (insideEdges != null)
//                DrawEdges(insideEdges, InsideEdgesColor);

//            // Draw newly inserted triangles
//            if (newTriangles != null)
//                DrawTriangles(newTriangles, NewTrianglesColor);

//            // Draw nodes
//            DrawNodes(delaunay.Nodes, NodeColor, 0.01f);
//        }

//        // Draw voronoi graph
//        if (voronoi != null)
//            DrawEdges(voronoi.Edges, VoronoiColor);
//    }

//    private void DrawNodes(IEnumerable<GraphNode> nodes, Color color, float radius)
//    {
//        // Remember original color, set new color
//        Color original = Gizmos.color;
//        Gizmos.color = color;

//        // Draw nodes
//        foreach (GraphNode node in nodes)
//            Gizmos.DrawSphere(node.Vector, radius);

//        // Reset color to original
//        Gizmos.color = original;
//    }

//    private void DrawEdges(IEnumerable<GraphEdge> edges, Color color)
//    {
//        // Remember original color, set new color
//        Color original = Gizmos.color;
//        Gizmos.color = color;

//        // Draw line between each node
//        foreach (GraphEdge edge in edges)
//            Gizmos.DrawLine(edge.Nodes[0].Vector, edge.Nodes[1].Vector);

//        // Reset color to original
//        Gizmos.color = original;
//    }

//    private void DrawTriangles(IEnumerable<GraphTriangle> triangles, Color color)
//    {
//        // Remember original color, set new color
//        Color original = Gizmos.color;
//        Gizmos.color = color;

//        // Draw line between each node
//        foreach (GraphTriangle triangle in triangles)
//        {
//            for (int i = 0; i < triangle.Nodes.Length - 1; i++)
//            {
//                for (int j = i + 1; j < triangle.Nodes.Length; j++)
//                {
//                    Gizmos.DrawLine(triangle.Nodes[i].Vector, triangle.Nodes[j].Vector);
//                }
//            }
//        }

//        // Reset color to original
//        Gizmos.color = original;
//    }

//    private void DrawCircumcircles(IEnumerable<GraphTriangle> triangles, Color color)
//    {
//        // Remember original color, set new color
//        Color original = UnityEditor.Handles.color;
//        UnityEditor.Handles.color = color;

//        // Draw circumcircle for each triangle
//        foreach (GraphTriangle triangle in triangles)
//            UnityEditor.Handles.DrawWireDisc(triangle.Circumcircle.Centre, -Vector3.forward, (float)triangle.Circumcircle.Radius);

//        // Reset color to original
//        UnityEditor.Handles.color = original;
//    }
//}
