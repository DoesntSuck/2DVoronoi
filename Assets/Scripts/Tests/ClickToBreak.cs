using UnityEngine;
using System.Linq;
using System.Collections.Generic;
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
                    Vector2 point = Geometry.RandomVectorFromTriangularDistribution(clickPosition, Radius);
                    while (!collider.OverlapPoint(point))
                        point = Geometry.RandomVectorFromTriangularDistribution(clickPosition, Radius);

                    points[i] = transform.InverseTransformPoint(point);
                }

                Break(points);
            }
        }

        // TODO: Send original mesh through Mesh clipper, so the REMAINS of it can be calculated

        void Break(Vector2[] points)
        {
            voronoi = new VoronoiTessellation();
            voronoi.Insert(points);

            // Clip mesh for each voronoi cell
            foreach (VoronoiCell clipCell in voronoi.Cells)
            {
                Graph clippedGraph = meshFilter.mesh.ToGraph();

                // Flatten edge collection into list of vectors to use as edge points
                List<Vector2> edgePoints = clipCell.Edges.SelectMany(e => e.Nodes.Select(n => n.Vector)).ToList();

                GraphClipper.Clip(clippedGraph, edgePoints, clipCell.Nuclei);

                CreateChunk(clippedGraph, clipCell.Nuclei);
            }

            GetComponent<Collider2D>().enabled = false;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Create a 2D, polygonal game object from a graph
        /// </summary>
        private void CreateChunk(Graph graph, Vector2 nuclei)
        {
            // Create chunk from prefab, add mesh
            GameObject chunk = Instantiate(ChunkPrefab, transform.position, Quaternion.identity) as GameObject;
            chunk.GetComponent<MeshFilter>().mesh = graph.ToMesh("Clipped Mesh");

            // Find all points external to the polygon
            IEnumerable<GraphNode> outsideNodes = graph.OutsideNodes();
            IOrderedEnumerable<GraphNode> orderedOutsideNodes = outsideNodes.OrderBy(n => n, new ClockwiseNodeComparer(nuclei));
            Vector2[] outsidePoints = orderedOutsideNodes.Select(n => n.Vector).ToArray();

            // Create polygon collider from points
            chunk.GetComponent<PolygonCollider2D>().points = outsidePoints;

        }
    }
}