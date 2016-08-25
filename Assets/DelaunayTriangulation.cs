using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Graph2D;

namespace Assets
{
    public class DelaunayTriangulation
    {
        /// <summary>
        /// The Graph data structure containing nodes, edges, and triangles
        /// </summary>
        public Graph Graph { get; private set; }

        private GraphNode[] superTriangleNodes;

        /// <summary>
        /// Delaunay Triangulation whose super triangle is sized to encompass the given incircle
        /// </summary>
        /// <param name="origin">Centre of the super triangle incircle</param>
        /// <param name="radius">Radius of the super triangle incircle</param>
        public DelaunayTriangulation(Vector2 origin, float radius)
        {
            Graph = new Graph();

            // Insert triangle large enough to encompass all vectors that will be inserted
            superTriangleNodes = GraphUtility.InsertSuperTriangle(Graph, origin, radius);
        }

        /// <summary>
        /// Inserts the given point to this Delaunay triangulation maintaining its Delaunay-ness
        /// </summary>
        public DelaunayTriangulation Insert(Vector2 vector)
        {
            // Add new node to graph
            GraphNode newNode = Graph.AddNode(vector);

            // Find guilty triangles
            List<GraphTriangle> guiltyTriangles = GraphUtility.WithinCircumcircles(Graph.Triangles, vector).ToList();

            // Seperate triangles into inside and outside constituent edges
            HashSet<GraphEdge> insideEdges;
            HashSet<GraphEdge> outsideEdges;
            GraphUtility.InsideOutsideEdges(out insideEdges, out outsideEdges, guiltyTriangles);

            // Remove guilty triangles from graph
            foreach (GraphTriangle triangle in guiltyTriangles)
                Graph.Remove(triangle);

            // Remove inside edges from graph
            foreach (GraphEdge insideEdge in insideEdges)
                Graph.Remove(insideEdge);

            // Triangulate hole left by removed edges
            foreach (GraphEdge outsideEdge in outsideEdges)
                Graph.CreateTriangle(outsideEdge, newNode);

            // Builder pattern: return self
            return this;
        }

        /// <summary>
        /// Inserts all vectors in the given enumeration
        /// </summary>
        public DelaunayTriangulation InsertRange(IEnumerable<Vector2> vectors)
        {
            // Insert each vector
            foreach (Vector2 vector in vectors)
                Insert(vector);

            // Builder pattern: return self
            return this;
        }

        /// <summary>
        /// Creates and returns the voronoi dual graph of this delaunay triangulation. A node is created for each triangle in this graph, the 
        /// node is position at its associated triangle's circumcentre. Adjacent triangles have their dual nodes connected by an edge.
        /// </summary>
        public VoronoiGraph CircumcircleDualGraph()
        {
            // Dict to associate triangles with nodes in dual graph
            Dictionary<GraphTriangle, GraphNode> triNodeDict = new Dictionary<GraphTriangle, GraphNode>();
            VoronoiGraph dualGraph = new VoronoiGraph();
            
            //
            // Add cell border nodes
            //
            
            // Create a node for each triangle circumcircle in THIS graph <- constitutes a cell border node
            foreach (GraphTriangle triangle in Graph.Triangles)
            {
                GraphNode node = dualGraph.AddNode(triangle.Circumcircle.Centre);
                triNodeDict.Add(triangle, node);    // Remeber the nodes association to its triangle
            }

            //
            // Add cell border edges
            //

            // Find triangles that share an edge, create an edge in dual graph connecting their associated nodes
            foreach (GraphTriangle triangle1 in Graph.Triangles)
            {
                // Compare each triangle to each other triangle
                foreach (GraphTriangle triangle2 in Graph.Triangles.Where(t => t != triangle1))
                {
                    foreach (GraphEdge edge in triangle1.Edges)
                    {
                        // Check if triangles share an edge
                        if (triangle2.Contains(edge))
                        {
                            // Get associated nodes
                            GraphNode node1 = triNodeDict[triangle1];
                            GraphNode node2 = triNodeDict[triangle2];

                            // Add an edge between them
                            dualGraph.AddEdge(node1, node2);
                        }
                    }
                }
            }

            //
            // Add cell nuclei 
            //

            // Each triangle using this node 
            foreach (GraphNode node in Graph.Nodes)
            {
                // Add node as a cell nuclei
                VoronoiCell cell = dualGraph.AddCell(node.Vector);

                // Add each of this nodes triangle's circumcircle nodes to cell
                foreach (GraphTriangle triangle in node.Triangles)
                    cell.AddNode(triNodeDict[triangle]);
            }

            return dualGraph;
        }

        public DelaunayTriangulation Build()
        {
            // Remove each super triangle node
            foreach (GraphNode superNode in superTriangleNodes)
                Graph.Remove(superNode);

            // Builder pattern: return self
            return this;
        }
    }
}
