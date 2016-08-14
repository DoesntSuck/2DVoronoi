using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Graph2D
{
    /// <summary>
    /// A 2d dimensional graph data structure containing nodes, edges and triangles.
    /// </summary>
    public class Graph
    {
        /// <summary>
        /// This graphs collection of nodes
        /// </summary>
        public HashSet<GraphNode> Nodes { get; private set; }

        /// <summary>
        /// This graphs collection of edges
        /// </summary>
        public HashSet<GraphEdge> Edges { get; private set; }

        /// <summary>
        /// This graphs collection of triangles
        /// </summary>
        public HashSet<GraphTriangle> Triangles { get; private set; }

        public Graph()
        {
            Nodes = new HashSet<GraphNode>();
            Edges = new HashSet<GraphEdge>();
            Triangles = new HashSet<GraphTriangle>();
        }

        /// <summary>
        /// Create and add a node that stores the given vector
        /// </summary>
        public GraphNode AddNode(Vector2 vector)
        {
            // Create, store, and return new node
            GraphNode newNode = new GraphNode(vector);
            Nodes.Add(newNode);
            return newNode;
        }

        /// <summary>
        /// Create and add an edge between the given nodes
        /// </summary>
        public GraphEdge AddEdge(GraphNode node1, GraphNode node2)
        {
            // Create new edge and add to list
            GraphEdge edge = new GraphEdge(node1, node2);
            Edges.Add(edge);

            // Add edge ref to each node
            foreach (GraphNode node in edge.Nodes)
                node.AddEdge(edge);

            return edge;
        }

        /// <summary>
        /// Adds a triangle connecting the given nodes. Will throw an exception if the given nodes to not form a triangle 
        /// </summary>
        public GraphTriangle AddTriangle(GraphNode a, GraphNode b, GraphNode c)
        {
            // Create triangle and add to triangle list
            GraphTriangle triangle = new GraphTriangle(a, b, c);
            Triangles.Add(triangle);

            // Add triangle ref to each of the triangle's nodes
            foreach (GraphNode node in triangle.Nodes)
                node.AddTriangle(triangle);

            // Add triangle ref to each of the triangle's edges
            foreach (GraphEdge edge in triangle.Edges)
                edge.AddTriangle(triangle);

            return triangle;
        }

        /// <summary>
        /// Creates a triangle connecting the given edge to the given node. Edges are inserted if necessary in order to create the triangle.
        /// </summary>
        public GraphTriangle CreateTriangle(GraphEdge edge, GraphNode node)
        {
            // Check if the neccesary edges already exist
            foreach (GraphNode edgeNode in edge.Nodes)
            {
                // If the graph doesn't contain the required edge, add it
                if (!ContainsEdge(edgeNode, node))
                    AddEdge(edgeNode, node);
            }

            // Create triangle between nodes
            return AddTriangle(edge.Nodes[0], edge.Nodes[1], node);
        }

        /// <summary>
        /// Removes reference to the given node from this graph. Edges and triangles connected to this node are also removed
        /// </summary>
        public void Remove(GraphNode node)
        {
            // Find and remove the node from the node list
            Nodes.Remove(node);

            // Find and remove all edges attached to the node being removed
            foreach (GraphEdge edge in node.Edges)
                Remove(edge);

            // Find and remove all triangles attached to the node being removed
            foreach (GraphTriangle triangle in node.Triangles)
                Remove(triangle);
        }

        /// <summary>
        /// Removes the reference to the given edge from this graph. Triangles connected to the edge are also removed; but, constituent nodes remain.
        /// </summary>
        public void Remove(GraphEdge edge)
        {
            // Remove edge from list of edges
            Edges.Remove(edge);

            // Remove ref to edge from its constituent nodes
            foreach (GraphNode node in edge.Nodes)
                node.RemoveEdge(edge);

            // Convert to array so original collection can be modified while looping
            GraphTriangle[] edgeTriangles = edge.Triangles.ToArray();

            // Iterate through all triangles containing edge
            foreach (GraphTriangle triangle in edgeTriangles)
                Remove(triangle);
        }

        /// <summary>
        /// Removes reference to the given triangle from this graph. Constituent nodes and edges are left in the graph.
        /// </summary>
        public void Remove(GraphTriangle triangle)
        {
            // Remove triangle from collection of triangles
            Triangles.Remove(triangle);

            // Remove ref to triangle from constituent nodes
            foreach (GraphNode node in triangle.Nodes)
                node.RemoveTriangle(triangle);

            // Remove ref to triangle from constituent edges
            foreach (GraphEdge edge in triangle.Edges)
                edge.RemoveTriangle(triangle);
        }

        /// <summary>
        /// Creates and returns the dual graph of this graph. A node is created for each triangle in this graph, the node is position at its
        /// associated triangle's circumcentre. Adjacent triangles have their dual nodes connected by an edge.
        /// </summary>
        public Graph CircumcircleDualGraph()
        {
            // Dict to associate triangles with nodes in dual graph
            Dictionary<GraphTriangle, GraphNode> triNodeDict = new Dictionary<GraphTriangle, GraphNode>();
            Graph dualGraph = new Graph();

            // Create a node for each triangle in THIS graph
            foreach (GraphTriangle triangle in Triangles)
            {
                GraphNode node = dualGraph.AddNode(triangle.Circumcircle.Centre);
                triNodeDict.Add(triangle, node);    // Remeber the nodes association to its triangle
            }

            // Find triangles that share an edge, create an edge in dual graph connecting their associated nodes
            foreach (GraphTriangle triangle1 in Triangles)
            {
                // Compare each triangle to each other triangle
                foreach (GraphTriangle triangle2 in Triangles.Where(t => t != triangle1))
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

            return dualGraph;
        }

        /// <summary>
        /// Checks if thie graph contains an edge between the given nodes
        /// </summary>
        public bool ContainsEdge(GraphNode a, GraphNode b)
        {
            // Check if any edge contains BOTH the given points
            foreach (GraphEdge edge in Edges)
            {
                if (edge.Contains(a, b))
                    return true;
            }

            return false;
        }
    }
}
