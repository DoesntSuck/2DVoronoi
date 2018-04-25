using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Graph2D;

namespace Assets
{
    [RequireComponent(typeof(PolygonCollider2D))]
    public class DelaunayTest : MonoBehaviour, SceneViewMouseMoveListener
    {
        public bool RemoveSuperTriangle = false;

        [Tooltip("Whether or not to draw each triangles circumcircle")]
        public bool Circumcircles;

        private List<Transform> children;
        public Graph Triangulation { get; private set; }
        private List<GraphTriangle> hoveredTriangles;

        void Awake()
        {
            // Get children that aren't a super triangle OR this transform
            children = GetComponentsInChildren<Transform>()
                .Where(c => c != transform)             // Not THIS transform
                .ToList();
        }

        // Use this for initialization
        void Start()
        {
            // Get transform positions
            List<Vector2> vectors = children.Select(c => (Vector2)c.position).ToList();

            Triangulation = DelaunayTriangulation.Create(vectors);
        }

        void OnDrawGizmos()
        {
            if (enabled)
            {
                // Get children that aren't a super triangle OR this transform
                children = GetComponentsInChildren<Transform>()
                    .Where(c => c != transform)             // Not THIS transform
                    .ToList();

                // Draw a node for each transform
                GraphDebug.DrawVectors(children.Select(c => c.position));

                // Draw the triangulation if it exists
                if (Triangulation != null)
                {
                    GraphDebug.Circumcircles = Circumcircles;
                    GraphDebug.DrawGraph(Triangulation);

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
        }

        public void MouseMoved(Ray worldSpaceRay)
        {
            // If triangulation has been calculated
            if (Triangulation != null)
            {
                // Raycast to find location on collider that has been hit
                RaycastHit2D hit = Physics2D.GetRayIntersection(worldSpaceRay);

                if (hit.collider != null)
                {
                    // List to store triangles that are hovered over (triangles can overlap, hence the list)
                    List<GraphTriangle> containingTriangles = new List<GraphTriangle>(); ;

                    // Check each triangle to see if it if the pointer is inside it
                    foreach (GraphTriangle triangle in Triangulation.Triangles)
                    {
                        if (Geometry2D.TriangleContains(hit.point, triangle.Nodes[0].Vector, triangle.Nodes[1].Vector, triangle.Nodes[2].Vector))
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