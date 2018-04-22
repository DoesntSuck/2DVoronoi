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

        public float NucleiGenerationRadius;
        public int MaxChunkCount;
        public float MinDistanceBetweenPoints;

        private VoronoiTessellation voronoi = new VoronoiTessellation();
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

                Break(GenerateNuclei(clickPosition, NucleiGenerationRadius, MaxChunkCount));
            }
        }

        private IEnumerable<Vector2> GenerateNuclei(Vector2 centre, float radius, int count)
        {
            List<Vector2> points = PointsWithinCollider(clickPosition, NucleiGenerationRadius, MaxChunkCount);
            IEnumerable<Vector2> closePointsExcluded = ExcludeClosePoints(points, MinDistanceBetweenPoints);
            IEnumerable<Vector2> transformedPoints = closePointsExcluded.Select(p => (Vector2)transform.InverseTransformPoint(p));

            return transformedPoints;
        }

        private List<Vector2> PointsWithinCollider(Vector2 centre, float radius, int count)
        {
            List<Vector2> points = new List<Vector2>();
            for (int i = 0; i < count; i++)
            {
                Vector2 point = Geometry.RandomVectorFromTriangularDistribution(centre, radius);
                if (collider.OverlapPoint(point))
                    points.Add(point);
            }

            return points;
        }

        private IEnumerable<Vector2> ExcludeClosePoints(List<Vector2> points, float minimumDistance)
        {
            // Min distance between points
            for (int i = 0; i < points.Count; i++)
            {
                Vector2 point = points[i];

                // Are there any points in the list that this point is too close to?
                bool tooClose = points.Except(i)
                                      .Where(p => Vector2.Distance(p, point) < minimumDistance)
                                      .Any();

                // If points isn't too close to any other points,
                // return it
                if (!tooClose)
                    yield return point;
            }
        }

        // TODO: Send original mesh through Mesh clipper, so the REMAINS of it can be calculated

        private void Break(IEnumerable<Vector2> points)
        {
            if (points.Count() > 1)
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

                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Create a 2D, polygonal game object from a graph
        /// </summary>
        private void CreateChunk(Graph graph, Vector2 nuclei)
        {
            // Create chunk from prefab, add mesh
            GameObject chunk = Instantiate(ChunkPrefab, transform.position, Quaternion.identity) as GameObject;
            chunk.GetComponent<MeshFilter>().mesh = graph.ToMesh("Clipped Mesh");
            chunk.transform.localScale = transform.localScale;

            // Find all points external to the polygon
            IEnumerable<GraphNode> outsideNodes = graph.OutsideNodes();
            IOrderedEnumerable<GraphNode> orderedOutsideNodes = outsideNodes.OrderBy(n => n, new ClockwiseNodeComparer(nuclei));
            Vector2[] outsidePoints = orderedOutsideNodes.Select(n => n.Vector).ToArray();

            // Create polygon collider from points
            chunk.GetComponent<PolygonCollider2D>().points = outsidePoints;

        }
    }
}