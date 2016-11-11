﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Graph2D;


public class GraphSplitterTest : MonoBehaviour
{
    public Transform inside;

    Transform[] edgeHandles;
    Mesh mesh;
    List<Graph> pieces;
    List<Dictionary<GraphNode, GraphNode>> piecesSplitNodes;

    void Awake()
    {
        // Get all child transforms, excluding this one.
        edgeHandles = GetComponentsInChildren<Transform>().Where(t => t != transform).ToArray();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Get mesh, convert to graph
            mesh = GetComponent<MeshFilter>().mesh;

            pieces = new List<Graph>();
            pieces.Add(new Graph(mesh));
            piecesSplitNodes = new List<Dictionary<GraphNode, GraphNode>>();

            for (int i = 0; i < edgeHandles.Length; i++)
            {
                Vector3 edgePoint1 = edgeHandles[i].position;
                Vector3 edgePoint2 = edgeHandles[(i + 1) % edgeHandles.Length].position;

                float insideSide = MathExtension.Side(edgePoint1, edgePoint2, inside.position);

                Graph insideGraph;
                Dictionary<GraphNode, GraphNode> splitNodes = GraphSplitter.Split(pieces.Last(), out insideGraph, edgePoint1, edgePoint2, insideSide);

                pieces.Add(insideGraph);
                piecesSplitNodes.Add(splitNodes);
            }

            CreateGameObject(pieces.Last());
            for (int i = 0; i < pieces.Count - 2; i++)
                pieces[i + 1].Stitch(pieces[i], piecesSplitNodes[i]);
            CreateGameObject(pieces[pieces.Count - 2]);

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

    void CreateGameObject(Graph graph)
    {
        GameObject graphObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Destroy(graphObject.GetComponent<MeshCollider>());
        graphObject.GetComponent<MeshFilter>().mesh = graph.ToMesh();
    }
}
