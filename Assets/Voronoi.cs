using UnityEngine;
using System.Collections;
using System.Linq;
using Graph2D;

namespace Assets
{
    public class Voronoi : MonoBehaviour
    {
        [Range(0, 1)]
        public float Spacing = 0.1f;

        public int NodeCount = 10;
        [ReadOnly]
        public int EdgeCount;
        [ReadOnly]
        public int TriangleCount;
        public Vector2[] vectors;

        public bool DrawCircumcircle = true;

        private DelaunayTriangulation triangulation;
        private Graph graph;
        private int vectorsIndex;
        private IEnumerator triangulationStepper;

        // Use this for initialization
        void Start()
        {
            // Randomly generate NodeCount number of vectors IF none have been input to vectors array
            if (vectors.Length == 0)
            {
                // Random vector within unit circle
                vectors = new Vector2[NodeCount];
                for (int i = 0; i < vectors.Length; i++)
                {
                    // Generate new vector to be inserted
                    Vector2 newVector = Random.insideUnitCircle;

                    // Get array sub array of vectors that have already been generated
                    Vector2[] others = vectors.SubArray(0, i);

                    // Generate a replacement vector until the new vector is not within range of any other vector
                    while (newVector.AnyWithinDistance(others, Spacing))
                        newVector = Random.insideUnitCircle;

                    vectors[i] = newVector;
                }
            }

            // Triangulation which will be passed vectors
            triangulation = new DelaunayTriangulation();
            triangulationStepper = triangulation.GetStepEnumerator(vectors[vectorsIndex++]);

            // Keep debug counts up to date
            EdgeCount = triangulation.Graph.Edges.Count;
            TriangleCount = triangulation.Graph.Triangles.Count;
        }

        public void OnDrawGizmos()
        {
            if (triangulation != null)
                DrawDelaunay();

            if (graph != null)
                DrawGraph();
        }

        private void DrawGraph()
        {
            Gizmos.color = Color.red;

            foreach (GraphEdge edge in graph.Edges)
                Gizmos.DrawLine(edge.Nodes[0].Vector, edge.Nodes[1].Vector);

            Gizmos.color = Color.white;
        }

        private void DrawDelaunay()
        {
            // Draw edges
            Gizmos.color = Color.yellow;

            foreach (GraphEdge edge in triangulation.Graph.Edges)
                Gizmos.DrawLine(edge.Nodes[0].Vector, edge.Nodes[1].Vector);

            Gizmos.color = Color.white;

            // Draw circumcircle
            if (DrawCircumcircle)
            {
                foreach (GraphTriangle triangle in triangulation.Graph.Triangles)
                {
                    UnityEditor.Handles.color = Color.red;
                    UnityEditor.Handles.DrawWireDisc(triangle.Circumcircle.Centre, -Vector3.forward, (float)triangle.Circumcircle.Radius);
                }
            }

            // Draw triangles
            foreach (GraphTriangle triangle in triangulation.Graph.Triangles)
            {
                for (int i = 0; i < triangle.Nodes.Length - 1; i++)
                {
                    for (int j = i + 1; j < triangle.Nodes.Length; j++)
                    {
                        Gizmos.DrawLine(triangle.Nodes[i].Vector, triangle.Nodes[j].Vector);
                    }
                }
            }

            // Draw inside edges in black
            if (triangulation.InsideEdges != null)
            {
                Gizmos.color = Color.black;

                foreach (GraphEdge edge in triangulation.InsideEdges)
                    Gizmos.DrawLine(edge.Nodes[0].Vector, edge.Nodes[1].Vector);

                Gizmos.color = Color.white;
            }

            // Draw outside edges in green
            if (triangulation.OutsideEdges != null)
            {
                Gizmos.color = Color.green;

                foreach (GraphEdge edge in triangulation.OutsideEdges)
                    Gizmos.DrawLine(edge.Nodes[0].Vector, edge.Nodes[1].Vector);

                Gizmos.color = Color.white;
            }

            // Draw triangles that are being inserted to fill hole
            if (triangulation.NewTriangles != null)
            {
                Gizmos.color = Color.cyan;

                foreach (GraphTriangle triangle in triangulation.NewTriangles)
                {
                    for (int i = 0; i < triangle.Nodes.Length - 1; i++)
                    {
                        for (int j = i + 1; j < triangle.Nodes.Length; j++)
                        {
                            Gizmos.DrawLine(triangle.Nodes[i].Vector, triangle.Nodes[j].Vector);
                        }
                    }
                }

                Gizmos.color = Color.white;
            }

            // Draw nodes
            foreach (GraphNode node in triangulation.Graph.Nodes)
                Gizmos.DrawSphere(node.Vector, 0.025f);
        }

        void Update()
        {
            // Move to next part of triangulation when the left mouse button is pressed
            if (Input.GetButtonDown("Fire1"))
            {
                // If there are no more steps...
                if (triangulationStepper == null)
                {
                    // If already built, get dual graph
                    if (triangulation.Built)
                    {
                        graph = triangulation.Graph.CircumcircleDualGraph();
                    }

                    // Build delaunay triangulation (remove super triangle)
                    else
                    {
                        // Remove super triangle, update edge and tri count
                        triangulation.Build();
                        EdgeCount = triangulation.Graph.Edges.Count;
                        TriangleCount = triangulation.Graph.Triangles.Count;
                    }
                }

                else
                {
                    // Proceed to next step, update edge and tri count
                    bool end = !triangulationStepper.MoveNext();
                    EdgeCount = triangulation.Graph.Edges.Count;
                    TriangleCount = triangulation.Graph.Triangles.Count;

                    // If current enumeratpr is out of steps
                    if (end)
                    {
                        // If there are more vectors to add...
                        if (vectorsIndex < vectors.Length)
                            triangulationStepper = triangulation.GetStepEnumerator(vectors[vectorsIndex++]);

                        // All enumerators are finished...
                        else
                            triangulationStepper = null;
                    }
                }
            }
        }
    }
}