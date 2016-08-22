using UnityEngine;
using System.Collections;
using Graph2D;

namespace Assets
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(Collider2D))]
    public class Breakable : MonoBehaviour
    {
        private Graph graph;

        void Awake()
        {
            // Get mesh from filter
            Mesh mesh = GetComponent<MeshFilter>().sharedMesh;

            // Mesh verts, edges, and tris, saed as graph
            graph = new Graph(mesh);
        }
        
        private void Break(Vector2 impactPoint, int nChunks, float fractureDistance)
        {


            // Create triangulation
            DelaunayTriangulation triangulation = new DelaunayTriangulation(impactPoint, fractureDistance);
        }
    }
}