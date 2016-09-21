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
        List<Graph> cells = VoronoiTessellation.Create(nuclei, false);

        Mesh mesh = GetComponent<MeshFilter>().mesh;
        foreach (Graph cell in cells)
        {
            Mesh clippedMesh = MeshClipper.Clip(mesh, cell);

            if (clippedMesh != null)                // If mesh is not completely cropped
            {
                GameObject chunk = Instantiate(ChunkPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                chunk.GetComponent<MeshFilter>().mesh = clippedMesh;
            }
        }
    }
}
