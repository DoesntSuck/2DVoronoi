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
        public HashSet<GraphEdge> Edges { get; protected set; }

        /// <summary>
        /// This graphs collection of triangles
        /// </summary>
        public HashSet<GraphTriangle> Triangles { get; protected set; }

        public Graph()
        {
            Nodes = new List<GraphNode>();
            Edges = new HashSet<GraphEdge>();
            Triangles = new HashSet<GraphTriangle>();
        }

        /// <summary>
        /// Creates a graph from the given mesh.
        /// </summary>
        public Graph(Mesh mesh) 
            : this()
        {
            // Add each vert as a node to graph
            foreach (Vector3 vert in mesh.vertices)
                AddNode(vert);

            // Create triangle using mesh tri indices as node indices
            for (int i = 0; i < mesh.triangles.Length - 2; i += 3)
            {
                GraphNode a = Nodes[mesh.triangles[i]];
                GraphNode b = Nodes[mesh.triangles[i + 1]];
                GraphNode c = Nodes[mesh.triangles[i + 2]];

                GraphEdge ab = AddEdge(a, b);
                GraphEdge ac = AddEdge(a, c);
                GraphEdge bc = AddEdge(b, c);

                AddTriangle(a, b, c);
            }
        }

        /// <summary>
        /// Create and add a node that stores the given vector
        /// </summary>
        public virtual GraphNode AddNode(Vector2 vector)
        {
            // Create, store, and return new node
            GraphNode newNode = new GraphNode(vector);
            Nodes.Add(newNode);
            return newNode;
        }

        /// <summary>
        /// Create and add an edge between the given nodes
        /// </summary>
        public virtual GraphEdge AddEdge(GraphNode node1, GraphNode node2)
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
        /// Adds a triangle connecting the given nodes. Will throw an exception if the given nodes do not form a triangle 
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
                if (!node.HasEdge(edgeNode))
                    AddEdge(edgeNode, node);
            }

            // Create triangle between nodes
            return AddTriangle(edge.Nodes[0], edge.Nodes[1], node);
        }

        public GraphTriangle CreateTriangle(GraphNode a, GraphNode b, GraphNode c)
        {
            // Add edges if need be
            if (!a.HasEdge(b)) AddEdge(a, b);
            if (!a.HasEdge(c)) AddEdge(a, c);
            if (!b.HasEdge(c)) AddEdge(b, c);

            // Create triangle
            return AddTriangle(a, b, c);
        }

        /// <summary>
        /// Removes reference to the given node from this graph. Edges and triangles connected to this node are also removed
        /// </summary>
        public virtual void Remove(GraphNode node)
        {
            // Find and remove the node from the node list
            Nodes.Remove(node);

            GraphEdge[] nodeEdges = node.Edges.ToArray();
            // Find and remove all edges attached to the node being removed
            foreach (GraphEdge edge in nodeEdges)
                Remove(edge);


            GraphTriangle[] nodeTriangles = node.Triangles.ToArray();
            // Find and remove all triangles attached to the node being removed
            foreach (GraphTriangle triangle in nodeTriangles)
                Remove(triangle);
        }

        /// <summary>
        /// Removes the reference to the given edge from this graph. Triangles connected to the edge are also removed; but, constituent nodes remain.
        /// </summary>
        public virtual void Remove(GraphEdge edge)
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

        public void Clear()
        {
            Nodes = new List<GraphNode>();
            Edges = new HashSet<GraphEdge>();
            Triangles = new HashSet<GraphTriangle>();
        }

        public GraphNode FindNode(Vector2 vector)
        {
            foreach (GraphNode node in Nodes)
            {
                if (node.Vector == vector)
                    return node;
            }

            return null;
        }
    }
}
