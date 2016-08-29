using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Graph2D
{
    public static class GraphUtility
    {
        public static Graph GraphFromMesh(Mesh mesh)
        {
            Graph graph = new Graph();

            // Add each vert as a node to graph
            foreach (Vector3 vert in mesh.vertices)
                graph.AddNode(vert);

            // Create triangle using mesh tri indices as node indices
            for (int i = 0; i < mesh.triangles.Length - 2; i += 3)
            {
                GraphNode a = graph.Nodes[mesh.triangles[i]];
                GraphNode b = graph.Nodes[mesh.triangles[i + 1]];
                GraphNode c = graph.Nodes[mesh.triangles[i + 2]];

                GraphEdge ab = graph.AddEdge(a, b);
                GraphEdge ac = graph.AddEdge(a, c);
                GraphEdge bc = graph.AddEdge(b, c);

                graph.AddTriangle(a, b, c);
            }

            return graph;
        }

        public static Mesh MeshFromGraph(Graph graph)
        {
            Mesh mesh = new Mesh();

            // Create dictionary of node, index pairs
            Dictionary<GraphNode, int> nodeIndexDict = new Dictionary<GraphNode, int>();
            List<Vector3> vertices = new List<Vector3>();

            // Add vert for each node in graph
            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                nodeIndexDict.Add(graph.Nodes[i], i);
                vertices.Add(graph.Nodes[i].Vector);
            }

            // Set mesh verts
            mesh.SetVertices(vertices);

            // create list to hold tris
            List<int> triIndices = new List<int>();

            // foreach tri, get its node indicies from dict, add nodes to list of tris
            foreach (GraphTriangle triangle in graph.Triangles)
            {
                foreach (GraphNode node in triangle.Nodes)
                    triIndices.Add(nodeIndexDict[node]);
            }

            // convert list to array, give to mesh
            mesh.SetTriangles(triIndices, 0);

            return mesh;
        }

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
