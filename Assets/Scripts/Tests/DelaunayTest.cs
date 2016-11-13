﻿using UnityEngine;
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
        private DelaunayTriangulation triangulation;
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
            Vector2[] vectors = children.Select(c => (Vector2)c.position).ToArray();

            triangulation = new DelaunayTriangulation(MathExtension.BoundingCircle(vectors));

            // Set collider points to the super triangle vectors
            GetComponent<PolygonCollider2D>().points = triangulation.SuperTriangle.Select(n => n.Vector).ToArray();

            // Insert vectors into triangulation
            triangulation.Insert(vectors);

            if (RemoveSuperTriangle)
                triangulation.RemoveSuperTriangle();

            print("Valid: " + triangulation.Validate());
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
                if (triangulation != null)
                {
                    GraphDebug.Circumcircles = Circumcircles;
                    GraphDebug.DrawGraph(triangulation.Graph);

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
            if (triangulation != null)
            {
                // Raycast to find location on collider that has been hit
                RaycastHit2D hit = Physics2D.GetRayIntersection(worldSpaceRay);

                if (hit.collider != null)
                {
                    // List to store triangles that are hovered over (triangles can overlap, hence the list)
                    List<GraphTriangle> containingTriangles = new List<GraphTriangle>(); ;

                    // Check each triangle to see if it if the pointer is inside it
                    foreach (GraphTriangle triangle in triangulation.Graph.Triangles)
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