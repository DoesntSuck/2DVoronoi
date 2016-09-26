using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Graph2D;

public class BreakTest : MonoBehaviour
{
    public GameObject ChunkPrefab;

    private List<Transform> children;

    void Awake()
    {
        children = GetComponentsInChildren<Transform>()     // Get all child transforms
            .Where(c => c != transform)                     // Except THIS transform
            .ToList();
    }

    //TODO: Voronoi needs bounds!

    void Start()
    {
        // Create Voronoi cells from child transform positions as nuclei
        Vector2[] nuclei = children.Select(c => (Vector2)c.position).ToArray();

        // Each point converts to the nuclei of a Voronoi cell
        VoronoiTessellation voronoi = new VoronoiTessellation(MathExtension.BoundingCircle(nuclei));
        voronoi.Insert(nuclei);

        Mesh mesh = GetComponent<MeshFilter>().mesh;
        foreach (Graph cell in voronoi.Cells)
        {
            Graph clippedGraph = MeshClipper.ClipAsGraph(mesh, cell);
            Mesh clippedMesh = clippedGraph.ToMesh("ClippedMesh");

            // If mesh is not completely cropped
            if (clippedGraph != null)
            {
                GameObject chunk = Instantiate(ChunkPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                chunk.GetComponent<MeshFilter>().mesh = clippedMesh;
                chunk.GetComponent<PolygonCollider2D>().points = clippedGraph.OutsideNodes().Select(n => n.Vector).ToArray();
            }
        }

        GetComponent<MeshRenderer>().enabled = false;
    }
}
