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

        public Vector2 Nuclei { get; set; }

        public int id { get; private set; }

        public Graph()
        {
            Nodes = new List<GraphNode>();
            Edges = new List<GraphEdge>();
            Triangles = new List<GraphTriangle>();
            id = GetHashCode();
        }

        /// <summary>
        /// Creates a graph from the given mesh.
        /// </summary>
        public Graph(Mesh mesh)
            : this()
        {
            // Add each vert as a node to graph
            foreach (Vector3 vert in mesh.vertices)
                CreateNode(vert);

            // Create triangle using mesh tri indices as node indices
            for (int i = 0; i < mesh.triangles.Length - 2; i += 3)
            {
                GraphNode a = Nodes[mesh.triangles[i]];
                GraphNode b = Nodes[mesh.triangles[i + 1]];
                GraphNode c = Nodes[mesh.triangles[i + 2]];

                GraphEdge ab = a.GetEdge(b) ?? CreateEdge(a, b);
                GraphEdge ac = a.GetEdge(c) ?? CreateEdge(a, c);
                GraphEdge bc = b.GetEdge(c) ?? CreateEdge(b, c);

                DefineTriangle(ab, ac, bc);
            }
        }

        /// <summary>
        /// Creates a mesh by converting this graphs nodes to verts and triangles to mesh triangles. Returns null if this graph is empty
        /// </summary>
        public Mesh ToMesh(string name = null)
        {
            // If graph is empty, do not return a mesh, return null
            if (Nodes.Count == 0 || Edges.Count == 0 || Triangles.Count == 0)
                return null;

            Mesh mesh = new Mesh() { name = name };

            // Create dictionary of node, index pairs
            Dictionary<GraphNode, int> nodeIndexDict = new Dictionary<GraphNode, int>();
            List<Vector3> vertices = new List<Vector3>();

            // Add vert for each node in graph
            for (int i = 0; i < Nodes.Count; i++)
            {
                nodeIndexDict.Add(Nodes[i], i);
                vertices.Add(Nodes[i].Vector);
            }

            // Set mesh verts
            mesh.SetVertices(vertices);

            // create list to hold tris
            List<int> triIndices = new List<int>();

            // foreach tri, get its node indicies from dict, add nodes to list of tris
            foreach (GraphTriangle triangle in Triangles)
            {
                triangle.OrderNodes();
                foreach (GraphNode node in triangle.Nodes)
                    triIndices.Add(nodeIndexDict[node]);
            }

            // convert list to array, give to mesh
            mesh.SetTriangles(triIndices, 0);
            mesh.RecalculateNormals();

            return mesh;
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

        public GraphTriangle DefineTriangle(GraphNode a, GraphNode b, GraphNode c)
        {
            // TODO: Error here during breaking. There is no edge between two of the nodes
            GraphTriangle triangle = new GraphTriangle(a.GetEdge(b), a.GetEdge(c), b.GetEdge(c));
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
        /// Adds a triangle connecting the given nodes. Will throw an exception if the given nodes do not form a triangle 
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
        public void Destroy(GraphNode node)
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
        public void Destroy(GraphEdge edge)
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

        public bool Contains(GraphNode node)
        {
            return Nodes.Contains(node);
        }

        public bool ContainsTriangle(GraphNode a, GraphNode b, GraphNode c)
        {
            // Check each triangle to see if it contains ALL of the given nodes
            foreach (GraphTriangle triangle in Triangles)
            {
                if (triangle.Contains(a, b, c))
                    return true;
            }

            // No triangle contains ALL of the given nodes
            return false;
        }

        // Keys = node in THIS graph - Values = node in OTHER graph
        public void Stitch(Graph other, Dictionary<GraphNode, GraphNode> stitchNodes)
        {
            GraphNode[] thisNodes = stitchNodes.Keys.ToArray();
            GraphNode[] otherNodes = stitchNodes.Values.ToArray();

            // Add triangles that DON"T use any of the stitch nodes
            Nodes.AddRange(other.Nodes.Where(n => !otherNodes.Contains(n)));
            Edges.AddRange(other.Edges.Where(e => !e.ContainsAny(otherNodes)));
            Triangles.AddRange(other.Triangles);

            // Add all triangles except if one of the stitchNodes uses it
            // If triangle uses a stitchNode from other... use stitch node from THIS instead
            // 
            foreach (KeyValuePair<GraphNode, GraphNode> stitchNodePair in stitchNodes)
            {
                GraphNode thisNode = stitchNodePair.Key;
                GraphNode otherNode = stitchNodePair.Value;

               
                thisNode.Switch(otherNode);

                Nodes.Remove(thisNode);
                Nodes.Add(otherNode);

                //// Each triangle attached to otherStitchNode needs to be instead attached to stitchNode
                //foreach (GraphTriangle triangle in otherNode.Triangles)
                //{
                //    // Update triangle edges that use OTHER node (so that they now this THIS node)
                //    GraphEdge[] edges = triangle.GetEdges(otherNode);
                //    foreach (GraphEdge triEdge in edges)
                //    {
                //        int edgeNodeIndex = triEdge.GetNodeIndex(otherNode);
                //        triEdge.SetNodeAtIndex(thisNode, edgeNodeIndex);
                //    }

                //    // Update THIS node to know about new edges
                //    thisNode.Edges.AddRange(edges);

                //    // Get the index of otherStitchNode in its attached edge's array of nodes
                //    int index = triangle.GetNodeIndex(otherNode);
                //    triangle.Nodes[index] = thisNode;     // Replace the element at that index with stitchNode

                //    // Update THIS node to know about new triangle
                //    thisNode.Triangles.Add(triangle);
                //}
            }


            //GraphNode[] nonDuplicateNodes = other.Nodes.Except(stitchNodes.Values).ToArray();


            // Nodes.AddRange(other.Nodes.Except(stitchNodes.Values.Where(n => Nodes.Contains(n))));        // Add non-duplicate nodes

            //Nodes.AddRange(other.Nodes.Except(stitchNodes.Values));                                       // Add non-duplicate nodes
            //Edges.AddRange(other.Edges.Where(e => e.Nodes.Intersect(stitchNodes.Values).Count() != 2));   // Add non-duplicate edges
            //Triangles.AddRange(other.Triangles);                                                          // Add all triangles

            //foreach (KeyValuePair<GraphNode, GraphNode> stitchNodePair in stitchNodes)
            //{
            //    // Each edge attached to otherStitchNode needs to be instead attached to stitchNode
            //    foreach (GraphEdge attachedEdge in stitchNodePair.Value.Edges)
            //    {
            //        // Get the index of otherStitchNode in its attached edge's array of nodes
            //        int index = Array.FindIndex(attachedEdge.Nodes, n => n == stitchNodePair.Value);
            //        attachedEdge.Nodes[index] = stitchNodePair.Key;     // Replace the element at that index with stitchNode
            //    }

            //    // Each triangle attached to otherStitchNode needs to be instead attached to stitchNode
            //    foreach (GraphTriangle attachedTriangle in stitchNodePair.Value.Triangles)
            //    {
            //        // Get the index of otherStitchNode in its attached edge's array of nodes
            //        int index = Array.FindIndex(attachedTriangle.Nodes, n => n == stitchNodePair.Value);
            //        attachedTriangle.Nodes[index] = stitchNodePair.Key;     // Replace the element at that index with stitchNode
            //    }
            //}
        }

        public List<GraphNode> OutsideNodes()
        {
            List<GraphNode> outsideNodes = new List<GraphNode>();

            IEnumerable<GraphEdge> outsideEdges = Edges.Where(e => e.Triangles.Count == 1);
            foreach (GraphEdge outsideEdge in outsideEdges)
                // Add nodes not already contained in outsideNodes
                outsideNodes.AddRange(outsideEdge.Nodes.Where(n => !outsideNodes.Contains(n)));

            ClockwiseNodeComparer nodeComparer = new ClockwiseNodeComparer(Nuclei);
            outsideNodes.Sort(nodeComparer);

            return outsideNodes;
        }
    }
}
