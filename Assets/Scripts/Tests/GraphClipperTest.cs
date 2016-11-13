using UnityEngine;
using System.Collections;
using Graph2D;
using System.Linq;

public class GraphClipperTest : MonoBehaviour
{
    public Transform clipShape1Parent;
    public Transform clipShape2Parent;
    public Transform inside1;
    public Transform inside2;

    Mesh mesh;
    public Transform[] edgeHandles1;
    public Transform[] edgeHandles2;
    Graph clipGraph1;
    Graph clipGraph2;

    void Awake()
    {
        // Get all child transforms, excluding this one.
        edgeHandles1 = clipShape1Parent.GetComponentsInChildren<Transform>().Where(t => t != clipShape1Parent).ToArray();
        edgeHandles2 = clipShape2Parent.GetComponentsInChildren<Transform>().Where(t => t != clipShape2Parent).ToArray();
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
            Graph outsideGraph;
            GraphClipper.Clip(original, clipGraph1, inside1.position, out insideGraph, out outsideGraph);

            CreateGameObject(insideGraph);
            //CreateGameObject(outsideGraph);

            // Second clip
            GraphClipper.Clip(outsideGraph, clipGraph2, inside2.position, out insideGraph, out outsideGraph);

            CreateGameObject(insideGraph);
            CreateGameObject(outsideGraph);
        }
    }

    void OnDrawGizmos()
    {
        if (inside1 != null) Gizmos.DrawSphere(inside1.position, 0.01f);
        if (inside2 != null) Gizmos.DrawSphere(inside2.position, 0.01f);

        if (clipShape1Parent != null)
        {
            edgeHandles1 = clipShape1Parent.GetComponentsInChildren<Transform>().Where(t => t != clipShape1Parent).ToArray();

            // Draw edge
            Gizmos.color = Color.red;
            for (int i = 0; i < edgeHandles1.Length; i++)
                Gizmos.DrawLine(edgeHandles1[i].position, edgeHandles1[(i + 1) % edgeHandles1.Length].position);
        }

        if (clipShape2Parent != null)
        {
            edgeHandles2 = clipShape2Parent.GetComponentsInChildren<Transform>().Where(t => t != clipShape2Parent).ToArray();

            // Draw edge
            Gizmos.color = Color.red;
            for (int i = 0; i < edgeHandles2.Length; i++)
                Gizmos.DrawLine(edgeHandles2[i].position, edgeHandles2[(i + 1) % edgeHandles2.Length].position);
        }
    }

    void CreateClipGraphs()
    {
        clipGraph1 = new Graph();
        for (int i = 0; i < edgeHandles1.Length - 1; i++)
        {
            GraphNode node1 = clipGraph1.CreateNode(edgeHandles1[i].position);
            GraphNode node2 = clipGraph1.CreateNode(edgeHandles1[i + 1].position);

            clipGraph1.CreateEdge(node1, node2);  
        }

        clipGraph1.CreateEdge(clipGraph1.Nodes.Last(), clipGraph1.Nodes.First());

        clipGraph2 = new Graph();
        for (int i = 0; i < edgeHandles2.Length - 1; i++)
        {
            GraphNode node1 = clipGraph2.CreateNode(edgeHandles2[i].position);
            GraphNode node2 = clipGraph2.CreateNode(edgeHandles2[i + 1].position);

            clipGraph2.CreateEdge(node1, node2);
        }

        clipGraph2.CreateEdge(clipGraph2.Nodes.Last(), clipGraph2.Nodes.First());
    }

    void CreateGameObject(Graph graph)
    {
        GameObject graphObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Destroy(graphObject.GetComponent<MeshCollider>());
        graphObject.GetComponent<MeshFilter>().mesh = graph.ToMesh();
    }
}
