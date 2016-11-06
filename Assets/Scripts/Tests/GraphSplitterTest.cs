using UnityEngine;
using System.Collections;
using System.Linq;
using Graph2D;


public class GraphSplitterTest : MonoBehaviour
{
    public Transform inside;

    Mesh mesh;
    Transform[] edgeHandles;

    void Awake()
    {
        // Get all child transforms, excluding this one.
        edgeHandles = GetComponentsInChildren<Transform>().Where(t => t != transform).ToArray();
    }

	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Get mesh, convert to graph
            mesh = GetComponent<MeshFilter>().mesh;
            Graph outsideGraph = new Graph(mesh);

            float insideSide = MathExtension.Side(edgeHandles[0].position, edgeHandles[1].position, inside.position);
            SplitGraph splitGraph1 = GraphSplitter.Split(outsideGraph, edgeHandles[0].position, edgeHandles[1].position, insideSide);

            insideSide = MathExtension.Side(edgeHandles[1].position, edgeHandles[2].position, inside.position);
            SplitGraph splitGraph2 = GraphSplitter.Split(splitGraph1.Inside, edgeHandles[1].position, edgeHandles[2].position, 1);

            CreateGameObject(splitGraph2.Inside);
            CreateGameObject(splitGraph2.Outside);
            CreateGameObject(splitGraph1.Outside);

            GetComponent<MeshRenderer>().enabled = false;
            // TODO: split again, using another line
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
            for (int i = 0; i < edgeHandles.Length - 1; i++)
                Gizmos.DrawLine(edgeHandles[i].position, edgeHandles[i + 1].position);
        }
    }

    void CreateGameObject(Graph graph)
    {
        GameObject graphObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Destroy(graphObject.GetComponent<MeshCollider>());
        graphObject.GetComponent<MeshFilter>().mesh = graph.ToMesh();
    }
}
