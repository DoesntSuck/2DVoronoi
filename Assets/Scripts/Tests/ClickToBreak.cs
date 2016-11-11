using UnityEngine;
using System.Linq;
using Graph2D;
using UnityEditor;

// TODO: Adjust chunk mesh vertices by nuclei position: nuclei is now the chunks position

namespace Assets
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(Collider2D))]
    public class ClickToBreak : MonoBehaviour
    {
        public GameObject ChunkPrefab;

        public float Radius;
        public int ChunkCount;

        private VoronoiTessellation voronoi;
        private MeshFilter meshFilter;
        private Vector3 clickPosition;
        private new Collider2D collider;

        void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            collider = GetComponent<Collider2D>();
        }

        void OnMouseDown()
        {
            Ray screenRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Raycast against this objects collider
            RaycastHit2D hitInfo = Physics2D.GetRayIntersection(screenRay, 100);
            if (hitInfo.collider != null)
            {
                // Set Origin to click location
                clickPosition = hitInfo.point;

                // Generate 'Count' number of random Vectors that tend towards Origin
                Vector2[] points = new Vector2[ChunkCount];
                for (int i = 0; i < ChunkCount; i++)
                {
                    Vector2 point = MathExtension.RandomVectorFromTriangularDistribution(clickPosition, Radius);
                    while (!collider.OverlapPoint(point))
                        point = MathExtension.RandomVectorFromTriangularDistribution(clickPosition, Radius);

                    points[i] = point;
                }

                Break(points);
            }
        }

        void OnDrawGizmos()
        {
            Handles.color = Color.cyan;
            Handles.DrawWireDisc(clickPosition, -Vector3.forward, Radius);

            if (voronoi != null)
            {
                GraphDebug.EdgeColour = Color.red;

                foreach (Graph cell in voronoi.Cells)
                    GraphDebug.DrawEdges(cell.Edges);
            }
        }

        // TODO: Send original mesh through Mesh clipper, so the REMAINS of it can be calculated

        void Break(Vector2[] points)
        {
            voronoi = new VoronoiTessellation();
            voronoi.Insert(points);

            // This is the graph that will be clipped, after being clipped this graph stores the area of graph inside the Voronoi cell
            Graph clipGraph = new Graph(meshFilter.mesh);

            foreach (Graph clipCell in voronoi.Cells)
            {
                // Each clip creates a new graph
                Graph outside;
                Graph inside;
                GraphClipper.Clip(clipGraph, clipCell, clipCell.Nuclei, out inside, out outside);

                // Instantiate 
                GameObject chunk = Instantiate(ChunkPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                chunk.GetComponent<MeshFilter>().mesh = clipGraph.ToMesh("Clipped Mesh");
                chunk.GetComponent<PolygonCollider2D>().points = clipGraph.OutsideNodes().Select(n => n.Vector).ToArray();

                // The remains is clipped next
                clipGraph = outside;
            }

            GetComponent<Collider2D>().enabled = false;
            gameObject.SetActive(false);
        }
    }
}