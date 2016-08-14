using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Graph2D;
using UnityEngine;

namespace Assets
{
    public class DelaunayTriangulationEnumerator : MonoBehaviour
    {
        [Tooltip("The extents of the super triangle that encorporates all nodes in the delaunay triangulation")]
        public Vector2 SuperTriangleExtents = new Vector2(2.5f, 2.5f);

        [Tooltip("The minimum distance between each randomly inserted node")]
        [Range(0, 1)]
        public float Spacing = 0.1f;

        [Tooltip("The number of random nodes to insert into the triangulation. If nodes have been specified via the vectors property, no random nodes will be added")]
        public int NodeCount = 10;

        [Tooltip("The number of edges present in the delaunay triangulation")]
        [ReadOnly]
        public int EdgeCount;

        [Tooltip("The number of triangles present in the delaunay triangulation")]
        [ReadOnly]
        public int TriangleCount;

        [Tooltip("The vector value of each node in the delaunay triangulation")]
        public Vector2[] vectors;

        [Tooltip("Should the delaunay triangulation draw the circumcircle of each triangle?")]
        public bool DrawCircumcircle = true;

        Graph delaunay;
        Graph voronoi;
        GraphNode[] superTriangleNodes;
        HashSet<GraphEdge> insideEdges;
        HashSet<GraphEdge> outsideEdges;
        List<GraphTriangle> newTriangles;
        IEnumerator stepper;

        void Awake()
        {
            // Create graph and insert super triangle
            delaunay = new Graph();
            superTriangleNodes = GraphUtility.InsertSuperTriangle(delaunay, Vector2.zero, SuperTriangleExtents.magnitude);

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

                    // Once vector passes test add it to array
                    vectors[i] = newVector;
                }
            }

            // Keep debug counts up to date
            EdgeCount = delaunay.Edges.Count;
            TriangleCount = delaunay.Triangles.Count;

            stepper = GetStepEnumerator(vectors);
        }

        void OnDrawGizmos()
        {
            // Draw delaunay graph
            if (delaunay != null)
            {
                // Draw circumcircles
                if (DrawCircumcircle)
                    DrawCircumcircles(delaunay.Triangles, Color.red);

                // Draw edges
                DrawEdges(delaunay.Edges, Color.yellow);

                // Draw triangles
                DrawTriangles(delaunay.Triangles, Color.white);

                // Draw outside edges
                if (outsideEdges != null)
                    DrawEdges(outsideEdges, Color.green);
                
                // Draw inside edges
                if (insideEdges != null)
                    DrawEdges(insideEdges, Color.black);

                // Draw newly inserted triangles
                if (newTriangles != null)
                    DrawTriangles(newTriangles, Color.cyan);

                // Draw nodes
                DrawNodes(delaunay.Nodes, Color.white, 0.025f);
            }

            // Draw voronoi graph
            if (voronoi != null)
                DrawEdges(voronoi.Edges, Color.red);
        }

        void Update()
        {
            // Move to next part of triangulation when the left mouse button is pressed
            if (Input.GetButtonDown("Fire1"))
            {
                bool end = !stepper.MoveNext();
                if (end)
                    voronoi = delaunay.CircumcircleDualGraph();
            }
        }

        private void DrawNodes(IEnumerable<GraphNode> nodes, Color color, float radius)
        {
            // Remember original color, set new color
            Color original = Gizmos.color;
            Gizmos.color = color;

            // Draw nodes
            foreach (GraphNode node in nodes)
                Gizmos.DrawSphere(node.Vector, radius);

            // Reset color to original
            Gizmos.color = original;
        }

        private void DrawEdges(IEnumerable<GraphEdge> edges, Color color)
        {
            // Remember original color, set new color
            Color original = Gizmos.color;
            Gizmos.color = color;

            // Draw line between each node
            foreach (GraphEdge edge in edges)
                Gizmos.DrawLine(edge.Nodes[0].Vector, edge.Nodes[1].Vector);

            // Reset color to original
            Gizmos.color = original;
        }

        private void DrawTriangles(IEnumerable<GraphTriangle> triangles, Color color)
        {
            // Remember original color, set new color
            Color original = Gizmos.color;
            Gizmos.color = color;

            // Draw line between each node
            foreach (GraphTriangle triangle in triangles)
            {
                for (int i = 0; i < triangle.Nodes.Length - 1; i++)
                {
                    for (int j = i + 1; j < triangle.Nodes.Length; j++)
                    {
                        Gizmos.DrawLine(triangle.Nodes[i].Vector, triangle.Nodes[j].Vector);
                    }
                }
            }

            // Reset color to original
            Gizmos.color = original;
        }

        private void DrawCircumcircles(IEnumerable<GraphTriangle> triangles, Color color)
        {
            // Remember original color, set new color
            Color original = UnityEditor.Handles.color;
            UnityEditor.Handles.color = color;

            // Draw circumcircle for each triangle
            foreach (GraphTriangle triangle in triangles)
                UnityEditor.Handles.DrawWireDisc(triangle.Circumcircle.Centre, -Vector3.forward, (float)triangle.Circumcircle.Radius);

            // Reset color to original
            UnityEditor.Handles.color = original;
        }

        private IEnumerator GetStepEnumerator(Vector2[] vectors)
        {
            foreach (Vector2 vector in vectors)
            {
                // Insert new node
                GraphNode newNode = delaunay.AddNode(vector);

                // Create list of triangles that have had their Delaunayness violated
                List<GraphTriangle> guiltyTriangles = GraphUtility.WithinCircumcircles(delaunay.Triangles, vector).ToList();

                // Get list of inside and outside edges
                HashSet<GraphEdge> insideEdges;
                HashSet<GraphEdge> outsideEdges;
                GraphUtility.InsideOutsideEdges(out insideEdges, out outsideEdges, guiltyTriangles);

                // Save to property so can view outside class
                this.insideEdges = insideEdges;
                this.outsideEdges = outsideEdges;
                yield return null;

                // Remove guilty triangle refs from graph
                foreach (GraphTriangle guiltyTriangle in guiltyTriangles)
                    delaunay.Remove(guiltyTriangle);

                // Remove inside edges leaving a hole in the triangulation
                foreach (GraphEdge insideEdge in insideEdges)
                    delaunay.Remove(insideEdge);

                this.insideEdges = null;        // Delete ref so inside edges are no longer drawn
                EdgeCount = delaunay.Edges.Count;
                TriangleCount = delaunay.Triangles.Count;
                yield return null;

                // Triangulate the hole
                newTriangles = new List<GraphTriangle>();
                foreach (GraphEdge outsideEdge in outsideEdges)
                {
                    GraphTriangle newTriangle = delaunay.CreateTriangle(outsideEdge, newNode);
                    newTriangles.Add(newTriangle);
                }

                this.outsideEdges = null;       // Delete ref so outside edges are no longer drawn
                EdgeCount = delaunay.Edges.Count;
                TriangleCount = delaunay.Triangles.Count;
                yield return null;
                newTriangles = null;            // Delete ref so new triangles are no longer drawn
            }
        }
    }
}
