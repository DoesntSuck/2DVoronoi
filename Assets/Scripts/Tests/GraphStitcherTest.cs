﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Graph2D;


public class GraphStitcherTest : MonoBehaviour
{
    Graph insideGraph;
    Graph outsideGraph;

    Mesh mesh;
    Transform[] edgeHandles;

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
            outsideGraph = new Graph(mesh);

            Dictionary<GraphNode, GraphNode> splitNodes = GraphSplitter.Split(outsideGraph, out insideGraph, edgeHandles[0].position, edgeHandles[1].position, 1);
            outsideGraph.Stitch(insideGraph, splitNodes);

            GetComponent<MeshFilter>().mesh = outsideGraph.ToMesh();
        }
    }

    void OnDrawGizmos()
    {
        if (edgeHandles != null && edgeHandles.Length == 2)
        {
            // Draw edge
            Gizmos.color = Color.red;
            Gizmos.DrawLine(edgeHandles[0].position, edgeHandles[1].position);
        }
    }

    void CreateGameObject(Graph graph)
    {
        GameObject graphObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Destroy(graphObject.GetComponent<MeshCollider>());
        graphObject.GetComponent<MeshFilter>().mesh = graph.ToMesh();
    }
}