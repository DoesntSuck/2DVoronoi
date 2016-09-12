using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Graph2D;

namespace Assets
{
    [RequireComponent(typeof(PolygonCollider2D))]
    public class DelaunayTest : MonoBehaviour, SceneViewMouseMoveListener
    {
        [Tooltip("The transforms that define the Delaunay Super Triangle")]
        public Transform[] SuperTriangle;

        [Tooltip("Whether or not to draw each triangles circumcircle")]
        public bool Circumcircles;

        private List<Transform> children;
        private Graph triangulation;
        private List<GraphTriangle> hoveredTriangles;

        void Awake()
        {
            // Get children that aren't a super triangle OR this transform
            children = GetComponentsInChildren<Transform>()
                .Except(SuperTriangle)                  // Except for the super triangle transforms
                .Where(c => c != transform)             // Not THIS transform
                .ToList();
        }

        // Use this for initialization
        void Start()
        {
            // Get transform positions
            Vector2[] vectors = children.Select(c => (Vector2)c.position).ToArray();

            // If there is no super triangle specified MAKE ONE
            if (SuperTriangle == null || SuperTriangle.Length != 3)
            {
                SuperTriangle = new Transform[3];

                // Create one and add a game object node for each
                Vector2[] superTriangleVectors = DelaunayTriangulation.CreateSuperTriangle(vectors);
                for (int i = 0; i < superTriangleVectors.Length; i++)
                {
                    // Create new node game object, add as child, and set position
                    GameObject superTriangleNode = new GameObject("SuperTriangleNode");
                    superTriangleNode.transform.parent = transform;
                    superTriangleNode.transform.position = superTriangleVectors[i];

                    // Save ref
                    SuperTriangle[i] = superTriangleNode.transform;
                }
            }


            // Set collider points to the super triangle vectors
            GetComponent<PolygonCollider2D>().points = SuperTriangle.Select(s => (Vector2)s.position).ToArray();

            // Create triangulation
            triangulation = DelaunayTriangulation.Create(vectors, SuperTriangle.Select(s => (Vector2)s.position).ToArray());
        }

        void OnDrawGizmos()
        {
            // Draw SuperTriangle
            if (SuperTriangle != null && SuperTriangle.Length == 3)
            {
                Gizmos.DrawLine(SuperTriangle[0].position, SuperTriangle[1].position);
                Gizmos.DrawLine(SuperTriangle[0].position, SuperTriangle[2].position);
                Gizmos.DrawLine(SuperTriangle[1].position, SuperTriangle[2].position);
            }

            // Get children that aren't a super triangle OR this transform
            children = GetComponentsInChildren<Transform>()
                .Except(SuperTriangle)                  // Except for the super triangle transforms
                .Where(c => c != transform)             // Not THIS transform
                .ToList();

            // Draw a node for each transform
            GraphDebug.DrawVectors(children.Select(c => c.position), Color.white, 0.025f);

            // Draw the triangulation if it exists
            if (triangulation != null)
            {
                GraphDebug.Circumcircles = Circumcircles;
                GraphDebug.DrawGraph(triangulation);

                // Highlight the triangles that are moused over
                if (hoveredTriangles != null)
                {
                    foreach (GraphTriangle triangle in hoveredTriangles)
                    {
                        GraphDebug.DrawTriangle(triangle, Color.green);
                        GraphDebug.DrawCircumcircle(triangle, Color.magenta);
                    }
                }

            }
        }

        public void MouseMoved(Ray worldSpaceRay)
        {
            // If triangulation has been calculated
            if (triangulation != null)
            {
                // Raycast to find location on collider that has been hit
                RaycastHit2D hit = Physics2D.GetRayIntersection(worldSpaceRay);

                if (hit.collider != null)
                {
                    // List to store triangles that are hovered over (triangles can overlap, hence the list)
                    List<GraphTriangle> containingTriangles = new List<GraphTriangle>(); ;

                    // Check each triangle to see if it if the pointer is inside it
                    foreach (GraphTriangle triangle in triangulation.Triangles)
                    {
                        if (MathExtension.TriangleContains(hit.point, triangle.Nodes[0].Vector, triangle.Nodes[1].Vector, triangle.Nodes[2].Vector))
                            containingTriangles.Add(triangle);
                    }

                    // Save ref to currently hovered on triangles
                    hoveredTriangles = containingTriangles;
                }

                // Nothing is hit
                else
                    hoveredTriangles = null;
            }
        }
    }
}