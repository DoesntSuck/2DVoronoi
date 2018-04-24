using System;
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
        public List<GraphNode> Nodes { get; protected set; }

        /// <summary>
        /// This graphs collection of edges
        /// </summary>
        public List<GraphEdge> Edges { get; protected set; }

        /// <summary>
        /// This graphs collection of triangles
        /// </summary>
        public List<GraphTriangle> Triangles { get; protected set; }

        public Graph()
        {
            Nodes = new List<GraphNode>();
            Edges = new List<GraphEdge>();
            Triangles = new List<GraphTriangle>();
        }

        /// <summary>
        /// Create and add a node that stores the given vector
        /// </summary>
        public virtual GraphNode CreateNode(Vector2 vector)
        {
            // Create, store, and return new node
            GraphNode newNode = new GraphNode(vector);
            Nodes.Add(newNode);
            return newNode;
        }

        /// <summary>
        /// Create and add an edge between the given nodes
        /// </summary>
        public virtual GraphEdge CreateEdge(GraphNode node1, GraphNode node2)
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
        public GraphTriangle CreateTriangle(GraphEdge edge, GraphNode node)
        {
            List<GraphEdge> otherEdges = new List<GraphEdge>();

            // Check if the neccesary edges already exist
            foreach (GraphNode edgeNode in edge.Nodes)
            {
                // If the graph doesn't contain the required edge, add it
                GraphEdge other = node.GetEdge(edgeNode);
                if (other == null)
                    other = CreateEdge(edgeNode, node);

                otherEdges.Add(other);
            }

            // Create triangle between nodes
            return DefineTriangle(edge, otherEdges[1], otherEdges[0]);
        }

        /// <summary>
        /// Adds a triangle between 3 connected nodes. Will throw an exception if the given nodes do not form a triangle 
        /// </summary>
        public GraphTriangle DefineTriangle(GraphEdge a, GraphEdge b, GraphEdge c)
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
        /// Removes reference to the given node from this graph. Edges and triangles connected to this node are also removed
        /// </summary>
        public virtual void Destroy(GraphNode node)
        {
            // Find and remove the node from the node list
            Nodes.Remove(node);

            GraphEdge[] nodeEdges = node.Edges.ToArray();
            // Find and remove all edges attached to the node being removed
            foreach (GraphEdge edge in nodeEdges)
                Destroy(edge);


            GraphTriangle[] nodeTriangles = node.Triangles.ToArray();
            // Find and remove all triangles attached to the node being removed
            foreach (GraphTriangle triangle in nodeTriangles)
                Remove(triangle);
        }

        /// <summary>
        /// Removes the reference to the given edge from this graph. Triangles connected to the edge are also removed; but, constituent nodes remain.
        /// </summary>
        public virtual void Destroy(GraphEdge edge)
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

        public IEnumerable<GraphNode> OutsideNodes()
        {
            List<GraphNode> outsideNodes = new List<GraphNode>();

            IEnumerable<GraphEdge> outsideEdges = Edges.Where(e => e.Triangles.Count == 1);
            foreach (GraphEdge outsideEdge in outsideEdges)
                // Add nodes not already contained in outsideNodes
                outsideNodes.AddRange(outsideEdge.Nodes.Where(n => !outsideNodes.Contains(n)));

            return outsideNodes;
        }
    }
}
