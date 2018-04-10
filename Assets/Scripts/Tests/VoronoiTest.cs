using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Graph2D;

namespace Assets
{
    public class VoronoiTest : MonoBehaviour
    {
        public bool RemoveSuperTriangle = false;
        public Color Colour = Color.cyan;

        private List<Transform> children;
        private VoronoiTessellation voronoi;

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

            voronoi = new VoronoiTessellation(Geometry.BoundingCircle(nuclei));
            voronoi.Insert(nuclei);
        }

        void OnDrawGizmos()
        {
            if (enabled)
            {
                IEnumerable<Transform> children = GetComponentsInChildren<Transform>()     // Get all child transforms
                    .Where(c => c != transform);                                           // Except THIS transform

                IEnumerable<Vector3> nuclei = children.Select(c => c.position);

                // Draw each nuclei
                GraphDebug.DrawVectors(nuclei);

                if (voronoi != null)
                {
                    GraphDebug.EdgeColour = Colour;

                    foreach (Graph cell in voronoi.Cells)
                        GraphDebug.DrawEdges(cell.Edges);
                }
            }
        }
    }
}