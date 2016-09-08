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
        public static List<Graph> Create(Vector2[] nuclei)
        {
            // Create super triangle
            Vector2[] superTriangle = null;

            // Create DelaunayTriangulation
            Graph delaunayTriangulation = DelaunayTriangulation.Create(nuclei, superTriangle);

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
                cells.Add(cell);

                // Dictionary to hold association between triangles in delaunay and circumcentre nodes in voronoi cell
                Dictionary<GraphTriangle, GraphNode> triNodeDict = new Dictionary<GraphTriangle, GraphNode>();

                // Create a node in voronoi cell for each triangle attached to the delaunay node
                foreach (GraphTriangle triangle in node.Triangles)
                {
                    GraphNode cellNode = cell.CreateNode(triangle.Circumcircle.Centre);
                    triNodeDict.Add(triangle, cellNode);
                }

                // Create edges between bordering triangles
                foreach (GraphTriangle triangle in node.Triangles)
                {
                    // Get collection of triangles that border this triangle
                    IEnumerable<GraphTriangle> borderingTriangles = node.Triangles.Where(t => t != triangle && t.SharesEdge(triangle));
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

            foreach (Graph cell in cells)
                OrderByClockwise(cell);

            // Return list of voronoi cells
            return cells;
        }

        /// <summary>
        /// Orders the nodes and edges in the given cell clockwise around the polygonal centre of the cell
        /// </summary>
        private static void OrderByClockwise(Graph cell)
        {
            // Calculate the centre of the polygon around which nodes will be ordered in a clockwise direction
            Vector2 polygonalCentre = MathExtension.PolygonCentre(cell.Nodes.Select(n => n.Vector).ToList());

            List<int> nodeOrder = new List<int>();
            List<int> edgeOrder = new List<int>();
            nodeOrder.Add(0);
            int walkerIndex = 0;

            // Iterate through all nodes in cells until reaching first node again
            while (walkerIndex != nodeOrder.First() || nodeOrder.Count != 1)
            {
                // Exchange index for ref to walker node
                GraphNode walker = cell.Nodes[walkerIndex];

                // Add current node to ordered list of nodes
                nodeOrder.Add(walkerIndex);

                // Get the next adjacent node in a clockwise order
                GraphNode next = GetAdjacentClockwiseNode(walker, polygonalCentre);
                if (next == null)
                    throw new ArgumentException("No clockwise adjacent node in cell");      // If there is no more clockwise nodes, something has gone wrong

                // Find the index of the 'next' node
                int nextIndex = cell.Nodes.FindIndex(n => n == next);
                if ((nodeOrder.Contains(nextIndex) && nextIndex != nodeOrder.First()) ||            // If revisiting a node that is not the first node
                    (nextIndex == nodeOrder.First() && nodeOrder.Count < cell.Nodes.Count))         // If revisiting the first node before completing the order
                    throw new ArgumentException("Revisiting a node, calculation has mussed up");    // Something has gone wrong!

                // Get the edge that connects walker and the next node
                GraphEdge connectingEdge = walker.GetEdge(next);

                // Find the index of the connecting edge in the cell's list of edges
                int connectingEdgeIndex = cell.Edges.FindIndex(e => e == connectingEdge);

                // Add edge index to ordered list of edges
                edgeOrder.Add(connectingEdgeIndex);
                    
                // Proceed to next node
                walkerIndex = nextIndex;
            }

            // Set order of nodes and edges in cell
            cell.SetNodeOrder(nodeOrder);
            cell.SetEdgeOrder(edgeOrder);
        }

        /// <summary>
        /// Returns the first node in the given nodes collection of nodes that is clockwise, based on the angle from the given polygonal centre
        /// </summary>
        private static GraphNode GetAdjacentClockwiseNode(GraphNode node, Vector2 polygonalCentre)
        {
            foreach (GraphEdge edge in node.Edges)
            {
                GraphNode adjacentNode = edge.GetOther(node);

                // Check which side the node is on
                float side = MathExtension.Side(node.Vector, polygonalCentre, adjacentNode.Vector);

                // If node is clockwise, return it
                if (side <= 0)
                    return adjacentNode;
            }

            // No clockwise nodes
            return null;
        }
    }
}
