using UnityEngine;
using System.Collections.Generic;
using Graph2D;
using System.Linq;

public class GraphClipperTest : MonoBehaviour
{
    Mesh mesh;
    List<Transform[]> edgeHandles;
    List<Transform> nuclei;
    List<Graph> clipGraphs;

    void Awake()
    {
        edgeHandles = new List<Transform[]>();
        nuclei = new List<Transform>();

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.gameObject.activeSelf)
            {
                IEnumerable<Transform> grandChildren = child.GetComponentsInChildren<Transform>().Where(t => t != child);

                edgeHandles.Add(grandChildren.Skip(1).ToArray());
                nuclei.Add(grandChildren.First());
            }
        }

        CreateClipGraphs();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GetComponent<MeshRenderer>().enabled = false;

            // Get mesh, convert to graph
            mesh = GetComponent<MeshFilter>().mesh;
            Graph original = new Graph(mesh);
            Graph insideGraph;
            Graph outsideGraph = original;

            foreach (Graph clipGraph in clipGraphs)
            {
                GraphClipper.Clip(outsideGraph, clipGraph, clipGraph.Nuclei, out insideGraph, out outsideGraph);
                CreateGameObject(insideGraph);
            }

            // TODO: after one shape clip, the graph has duplicate nodes.... maybe stitch is incorrect
            CreateGameObject(outsideGraph);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.gameObject.activeSelf)
            {
                Transform[] grandChildren = child.GetComponentsInChildren<Transform>().Where(t => t != child).ToArray();

                Gizmos.DrawSphere(grandChildren.First().position, 0.01f);

                grandChildren = grandChildren.Skip(1).ToArray();

                // Draw edge
                for (int j = 0; j < grandChildren.Length; j++)
                    Gizmos.DrawLine(grandChildren[j].position, grandChildren[(j + 1) % grandChildren.Length].position);
            }
        }
    }

    void CreateClipGraphs()
    {
        clipGraphs = new List<Graph>(edgeHandles.Count);
        for (int i = 0; i < edgeHandles.Count; i++)
        {
            Graph graph = new Graph();
            graph.Nuclei = nuclei[i].position;
            for (int j = 0; j < edgeHandles[i].Length - 1; j++)
            {
                GraphNode node1 = graph.CreateNode(edgeHandles[i][j].position);
                GraphNode node2 = graph.CreateNode(edgeHandles[i][j + 1].position);

                graph.CreateEdge(node1, node2);
            }
            graph.CreateEdge(graph.Nodes.Last(), graph.Nodes.First());

            clipGraphs.Add(graph);
        }
    }

    void CreateGameObject(Graph graph)
    {
        GameObject graphObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Destroy(graphObject.GetComponent<MeshCollider>());
        graphObject.GetComponent<MeshFilter>().mesh = graph.ToMesh();
    }
}
