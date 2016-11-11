using UnityEngine;
using System.Collections;
using Graph2D;
using System.Linq;

public class GraphClipperTest : MonoBehaviour
{
    public Transform inside;

    Mesh mesh;
    Transform[] edgeHandles;
    Graph clipGraph;

    void Awake()
    {
        // Get all child transforms, excluding this one.
        edgeHandles = GetComponentsInChildren<Transform>().Where(t => t != transform).ToArray();
        CreateClipGraph();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Get mesh, convert to graph
            mesh = GetComponent<MeshFilter>().mesh;

            Graph original = new Graph(mesh);
            Graph insideGraph;
            Graph outsideGraph;
            GraphClipper.Clip(original, clipGraph, inside.position, out insideGraph, out outsideGraph);

            CreateGameObject(insideGraph);
            CreateGameObject(outsideGraph);

            GetComponent<MeshRenderer>().enabled = false;
        }
    }

    void OnDrawGizmos()
    {
        if (inside != null)
            Gizmos.DrawSphere(inside.position, 0.01f);

        if (edgeHandles != null)
        {
            // Draw edge
            Gizmos.color = Color.red;
            for (int i = 0; i < edgeHandles.Length; i++)
                Gizmos.DrawLine(edgeHandles[i].position, edgeHandles[(i + 1) % edgeHandles.Length].position);
        }
    }

    void CreateClipGraph()
    {
        clipGraph = new Graph();
        for (int i = 0; i < edgeHandles.Length - 1; i++)
        {
            GraphNode node1 = clipGraph.CreateNode(edgeHandles[i].position);
            GraphNode node2 = clipGraph.CreateNode(edgeHandles[i + 1].position);

            clipGraph.CreateEdge(node1, node2);  
        }

        clipGraph.CreateEdge(clipGraph.Nodes.Last(), clipGraph.Nodes.First());
    }

    void CreateGameObject(Graph graph)
    {
        GameObject graphObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Destroy(graphObject.GetComponent<MeshCollider>());
        graphObject.GetComponent<MeshFilter>().mesh = graph.ToMesh();
    }
}
