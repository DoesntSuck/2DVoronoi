using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graph2D
{
    public static class GraphExtension
    {
        /// <summary>
        /// Creates a mesh by converting this graphs nodes to verts and triangles to 
        /// mesh triangles. Returns null if this graph is empty.
        /// </summary>
        public static Mesh ToMesh(this Graph graph, string name = null)
        {
            // If graph is empty, do not return a mesh, return null
            if (graph.Nodes.Count == 0 || graph.Edges.Count == 0 || graph.Triangles.Count == 0)
                return null;

            Mesh mesh = new Mesh() { name = name };

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
            mesh.RecalculateNormals();

            return mesh;
        }

        /// <summary>
        /// Converts the given mesh to a graph. Verts become graphNodes,
        /// and mesh triangles become graphTriangles
        /// </summary>
        public static Graph ToGraph(this Mesh mesh)
        {
            Graph graph = new Graph();

            // Add each vert as a node to graph
            foreach (Vector3 vert in mesh.vertices)
                graph.CreateNode(vert);

            // Create triangle using mesh tri indices as node indices
            for (int i = 0; i < mesh.triangles.Length - 2; i += 3)
            {
                GraphNode a = graph.Nodes[mesh.triangles[i]];
                GraphNode b = graph.Nodes[mesh.triangles[i + 1]];
                GraphNode c = graph.Nodes[mesh.triangles[i + 2]];

                GraphEdge ab = graph.CreateEdge(a, b);
                GraphEdge ac = graph.CreateEdge(a, c);
                GraphEdge bc = graph.CreateEdge(b, c);

                graph.DefineTriangle(ab, ac, bc);
            }

            return graph;
        }
    }
}