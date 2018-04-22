using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Graph2D
{
    public class VoronoiTessellation
    {
        /// <summary>
        /// The Delaunay Triangulation of which this Voronoi Tessellation is the dual graph.
        /// </summary>
        public DelaunayTriangulation Triangulation { get; private set; }

        /// <summary>
        /// Gets the cells in this Voronoi Tesellation. Calculated as the dual graph 
        /// of the Delaunay triangulation. Only completed cells are returned.
        /// </summary>
        public List<VoronoiCell> Cells { get { return cells.Where(c => c.IsPolygon).ToList(); } }

        private List<VoronoiCell> cells;

        /// <summary>
        /// A new Voronoi tesellation with an arbitrarily large bounding triangle.
        /// </summary>
        public VoronoiTessellation()
        {
            Triangulation = new DelaunayTriangulation();
        }

        /// <summary>
        /// Creates a new Voronoi tesellation with the given bounds. All inserted nuclei are assumed to be contained
        /// within the bounds.
        /// </summary>
        public VoronoiTessellation(Circle bounds)
        {
            Triangulation = new DelaunayTriangulation(bounds);
        }
        
        private void ValidateNuclei(Vector2[] nuclei)
        {
            for (int i = 0; i < nuclei.Length; i++)
            {
                // Check if any of the vectors are the same or close enough to be the same
                if (nuclei.Except(i).Where(n => Vector2.Distance(n, nuclei[i]) <= float.Epsilon).Any())
                    throw new System.ArgumentException("Nuclei are too close together");
            }
        }

        /// <summary>
        /// Inserts the given nuclei into this Voronoi tesellation, recaclulating the nuclei cells.
        /// </summary>
        public void Insert(IEnumerable<Vector2> nuclei)
        {
            //ValidateNuclei(nuclei);

            // Insert all the points
            foreach (Vector2 nucleus in nuclei)
                Triangulation.Insert(nucleus);

            // THEN recalculate the cells
            GenerateDualGraph();
        }

        /// <summary>
        /// Inserts the given nucleus into this Voronoi tesellation, recalculating the cells. If inserting multiple
        /// nuclei it is recommended to insert them all at once.
        /// </summary>
        public void Insert(Vector2 nucleus)
        {
            // Insert point
            Triangulation.Insert(nucleus);

            // THEN recalculate the cells
            GenerateDualGraph();
        }

        /// <summary>
        /// Calculate the Voronoi tesellation which is the dual graph of the Delaunay triangulation property.
        /// </summary>
        private void GenerateDualGraph()
        {
            // TODO: Where a cell edge intersects with supertriangle edge, truncate cell edge

            // Convert to Voronoi Cells
            cells = new List<VoronoiCell>();

            foreach (GraphNode node in Triangulation.Graph.Nodes)
            {
                // Create a new voronoi cell, add to list of cells
                VoronoiCell cell = new VoronoiCell(node.Vector);
                cells.Add(cell);

                // Dictionary to hold association between triangles in delaunay and circumcentre nodes in voronoi cell
                Dictionary<GraphTriangle, GraphNode> triNodeDict = new Dictionary<GraphTriangle, GraphNode>();

                // Create a node in voronoi cell for each triangle attached to the delaunay node
                foreach (GraphTriangle triangle in node.Triangles)
                {
                    GraphNode cellNode = cell.CreateNode(triangle.Circumcircle.Centre);
                    triNodeDict.Add(triangle, cellNode);
                }

                HashSet<GraphTriangle> visitedTriangles = new HashSet<GraphTriangle>();
                // Create edges between bordering triangles
                foreach (GraphTriangle triangle in node.Triangles)
                {
                    visitedTriangles.Add(triangle);
                    // Get collection of triangles that border this triangle
                    IEnumerable<GraphTriangle> borderingTriangles = node.Triangles.Where(t => !visitedTriangles.Contains(t) && t.SharesEdge(triangle));
                    foreach (GraphTriangle borderingTriangle in borderingTriangles)
                    {
                        // Get triangles' associated node in this cell
                        GraphNode node1 = triNodeDict[triangle];
                        GraphNode node2 = triNodeDict[borderingTriangle];

                        // Add an edge between the two nodes
                        cell.CreateEdge(node1, node2);
                    }
                }
            }
        }
    }
}
