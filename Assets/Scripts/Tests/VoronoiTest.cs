using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Graph2D;

namespace Assets
{
    public class VoronoiTest : MonoBehaviour, SceneViewMouseMoveListener
    {
        private List<Transform> children;
        private List<Graph> cells;

        private List<Graph> hoveredCells;


        // TODO: Need to add polygon collider, to allow for mouse hover
        public void MouseMoved(Ray worldSpaceRay)
        {
            // If triangulation has been calculated
            if (cells != null)
            {
                // Raycast to find location on collider that has been hit
                RaycastHit2D hit = Physics2D.GetRayIntersection(worldSpaceRay);

                if (hit.collider != null)
                {
                    // List to store triangles that are hovered over (triangles can overlap, hence the list)
                    List<Graph> containingCells = new List<Graph>(); ;

                    // Check each triangle to see if it if the pointer is inside it
                    foreach (Graph cell in cells)
                    {
                        if (MathExtension.ContainsPoint(cell.Nodes.Select(n => n.Vector).ToArray(), hit.point))
                            containingCells.Add(cell);
                    }

                    // Save ref to currently hovered on triangles
                    hoveredCells = containingCells;
                }

                // Nothing is hit
                else
                    hoveredCells = null;
            }
        }

        void Awake()
        {
            children = GetComponentsInChildren<Transform>()     // Get all child transforms
                .Where(c => c != transform)                     // Except THIS transform
                .ToList();
        }

        // Use this for initialization
        void Start()
        {
            // Create Voronoi cells from child transform positions as nuclei
            Vector2[] nuclei = children.Select(c => (Vector2)c.position).ToArray();
            cells = VoronoiTessellation.Create(nuclei);
        }

        void OnDrawGizmos()
        {
            IEnumerable<Transform> children = GetComponentsInChildren<Transform>()     // Get all child transforms
                .Where(c => c != transform);                                           // Except THIS transform

            IEnumerable<Vector3> nuclei = children.Select(c => c.position);

            // Draw each nuclei
            GraphDebug.DrawVectors(nuclei, Color.white, 0.025f);

            if (cells != null)
            {
                GraphDebug.EdgeColour = Color.white;

                foreach (Graph cell in cells)
                    GraphDebug.DrawEdges(cell.Edges);

                if (hoveredCells != null)
                {
                    GraphDebug.EdgeColour = Color.magenta;

                    foreach (Graph cell in cells)
                        GraphDebug.DrawEdges(cell.Edges);
                }
            }
        }
    }
}