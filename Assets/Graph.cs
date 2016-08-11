using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets
{
    public class Graph
    {
        public HashSet<GraphNode> Nodes { get; private set; }
        public HashSet<GraphEdge> Edges { get; private set; }
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
        /// Creates a triangle connecting the given edge to the given node. Edges are inserted if necessary in order to create the triangle.
        /// </summary>
        public GraphTriangle AddTriangle(GraphEdge edge, GraphNode node)
        {
            // Check if the neccesary edges already exist
            foreach (GraphNode edgeNode in edge.Nodes)
            {
                if (!ContainsEdge(edgeNode, node))
                    AddEdge(edgeNode, node);
            }

            // Create triangle between nodes
            return AddTriangle(edge.Nodes[0], edge.Nodes[1], node);
        }

        /// <summary>
        /// Adds a triangle connecting the given nodes. DOES NOT CREATE EDGES.
        /// </summary>
        public GraphTriangle AddTriangle(GraphNode a, GraphNode b, GraphNode c)
        {
            // TODO: Create edges where necessary

            // Create triangle and add to triangle list
            GraphTriangle triangle = new GraphTriangle(a, b, c);
            Triangles.Add(triangle);

            // Add triangle ref to each of the triangle's nodes
            foreach (GraphNode node in triangle.Nodes)
                node.AddTriangle(triangle);

            // Add triangle ref to each of the triangle's edges
            foreach (GraphEdge edge in Edges)
                edge.AddTriangle(triangle);

            return triangle;
        }

        /// <summary>
        /// Removes the given node from this graph. Edges and triangles connected to this node are also removed
        /// </summary>
        public void Remove(GraphNode node)
        {
            // Find and remove the node from the node list
            Nodes.Remove(node);

            // Find and remove all edges attached to the node being removed
            foreach (GraphEdge edge in node.Edges)
            {
                Edges.Remove(edge);

                // Remove ref to edge from its constituent nodes
                foreach (GraphNode edgeNode in edge.Nodes.Where(n => n != node))
                    edgeNode.RemoveEdge(edge);
            }

            // Find and remove all triangles attached to the node being removed
            foreach (GraphTriangle triangle in node.Triangles)
            {
                // Remove triangle from triangle list
                Triangles.Remove(triangle);

                // Remove ref to triangle from its constituent nodes
                foreach (GraphNode triangleNode in triangle.Nodes.Where(n => n != node))
                    triangleNode.RemoveTriangle(triangle);

                // Remove ref to triangle from its constituent edges
                foreach (GraphEdge edge in triangle.Edges)
                    edge.RemoveTriangle(triangle);
            }
        }

        /// <summary>
        /// Removes the given edge from this graph. Triangles connected to the edge are also removed
        /// </summary>
        public void Remove(GraphEdge edge)
        {
            // Remove edge from list of edges
            Edges.Remove(edge);

            // Remove ref to edge from its constituent nodes
            foreach (GraphNode node in edge.Nodes)
                node.RemoveEdge(edge);

            GraphTriangle[] triangles = edge.Triangles.ToArray();
            // Iterate through all triangles containing edge
            foreach (GraphTriangle triangle in triangles)
                edge.RemoveTriangle(triangle);
        }

        /// <summary>
        /// Removes the given triangle from this graph
        /// </summary>
        public void Remove(GraphTriangle triangle)
        {
            Triangles.Remove(triangle);

            // Remove ref to triangle from constituent nodes
            foreach (GraphNode node in triangle.Nodes)
                node.RemoveTriangle(triangle);

            // Remove ref to triangle from constituent edges
            foreach (GraphEdge edge in triangle.Edges)
                edge.RemoveTriangle(triangle);
        }

        public Graph DualGraph()
        {
            Dictionary<GraphTriangle, GraphNode> triNodeDict = new Dictionary<GraphTriangle, GraphNode>();
            Graph dualGraph = new Graph();

            foreach (GraphTriangle triangle in Triangles)
            {
                GraphNode node = dualGraph.AddNode(triangle.Circumcircle.Centre);
                triNodeDict.Add(triangle, node);
            }

            // Find triangles that this triangle shares an edge with, create an edge connecting their circumcircles
            foreach (GraphTriangle triangle1 in Triangles)
            {
                foreach (GraphTriangle triangle2 in Triangles.Where(t => t != triangle1))
                {
                    foreach (GraphEdge edge in triangle1.Edges)
                    {
                        if (triangle2.Contains(edge))
                        {
                            GraphNode node1 = triNodeDict[triangle1];
                            GraphNode node2 = triNodeDict[triangle2];

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
