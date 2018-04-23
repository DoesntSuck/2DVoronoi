using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Graph2D
{
    public class VoronoiTessellation
    {
        /// <summary>
        /// Gets the cells in this Voronoi Tesellation. Calculated as the dual graph 
        /// of the Delaunay triangulation. Only completed cells are returned.
        /// </summary>
        public List<VoronoiCell> Cells { get { return cells.Where(c => c.IsPolygon).ToList(); } }

        private List<VoronoiCell> cells;

        private Graph delaunayTriangulation;

        public VoronoiTessellation(IEnumerable<Vector2> nuclei, Vector2[] superTriangle)
        {
            delaunayTriangulation = DelaunayTriangulation.Create(nuclei, superTriangle, true);
            GenerateDualGraph();
        }

        public VoronoiTessellation(IEnumerable<Vector2> nuclei)
        {
            delaunayTriangulation = DelaunayTriangulation.Create(nuclei, true);
            GenerateDualGraph();
        }

        public VoronoiTessellation(IEnumerable<Vector2> nuclei, Circle bounds)
        {
            delaunayTriangulation = DelaunayTriangulation.Create(nuclei, bounds, true);
            GenerateDualGraph();
        }

        public VoronoiTessellation(Graph delaunayTriangulation)
        {
            this.delaunayTriangulation = delaunayTriangulation;
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

            foreach (GraphNode node in delaunayTriangulation.Nodes)
            {
                // Each node in delaunay is the nuclei of a voronoi cell
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
