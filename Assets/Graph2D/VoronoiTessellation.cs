using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Graph2D
{
    // TODO: No VoronoiCell class -> Create() returns List<Graph>
    // Create() constructs cell as graph, then calculates polygonal centre AND orders graph nodes / edges by clockwise

    public static class VoronoiTessellation
    {
        /// <summary>
        /// Creates Voronoi cells with the given nuclei. Each nuclei corresponds to a single Voronoi cell. Each point inside a Voronoi cell is
        /// closer to its nuclei that any other cells nueclei.
        /// </summary>
        public static List<Graph> Create(Vector2[] nuclei, bool removeSuperTriangle = false)
        {
            // Create DelaunayTriangulation
            Graph delaunayTriangulation = DelaunayTriangulation.Create(nuclei, removeSuperTriangle);

            // Convert to Voronoi Cells
            List<Graph> cells = Create(delaunayTriangulation);

            // Return list of voronoi cells
            return cells;
        }

        /// <summary>
        /// Creates Voronoi cells from the given Delaunay triangulation. The Voronoi tessellation is equal to the dual graph of the Delaunay 
        /// triangulation where the circumcentre of each triangle becomes a node in the Voronoi graph, and each bordering triangle in the Delaunay
        /// graph corresponds to an edge in the Voronoi dual.
        /// </summary>
        public static List<Graph> Create(Graph delaunayTriangulation)
        {
            // Convert to Voronoi Cells
            List<Graph> cells = new List<Graph>();

            foreach (GraphNode node in delaunayTriangulation.Nodes)
            {
                // Create a new voronoi cell add to list of cells
                Graph cell = new Graph();
                cell.Nuclei = node.Vector;
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

            return cells;
        }
    }
}
