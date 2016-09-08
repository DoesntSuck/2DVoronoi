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

                GraphEdge ab = CreateEdge(a, b);
                GraphEdge ac = CreateEdge(a, c);
                GraphEdge bc = CreateEdge(b, c);

                DefineTriangle(a, b, c);
            }
        }

        /// <summary>
        /// Creates a mesh by converting this graphs nodes to verts and triangles to mesh triangles. Returns null if this graph is empty
        /// </summary>
        public Mesh ToMesh()
        {
            // If graph is empty, do not return a mesh, return null
            if (Nodes.Count == 0 || Edges.Count == 0 || Triangles.Count == 0)
                return null;

            Mesh mesh = new Mesh();

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
                foreach (GraphNode node in triangle.Nodes)
                    triIndices.Add(nodeIndexDict[node]);
            }

            // convert list to array, give to mesh
            mesh.SetTriangles(triIndices, 0);

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
            // Check if the neccesary edges already exist
            foreach (GraphNode edgeNode in edge.Nodes)
            {
                // If the graph doesn't contain the required edge, add it
                if (!node.HasEdge(edgeNode))
                    CreateEdge(edgeNode, node);
            }

            // Create triangle between nodes
            return DefineTriangle(edge.Nodes[0], edge.Nodes[1], node);
        }

        /// <summary>
        /// Adds a triangle connecting the given nodes. Will throw an exception if the given nodes do not form a triangle 
        /// </summary>
        public GraphTriangle DefineTriangle(GraphNode a, GraphNode b, GraphNode c)
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

        /// <summary>
        /// Sets the order of nodes in this graphs collection of nodes according to the order of the indices in the given list
        /// </summary>
        public void SetNodeOrder(List<int> indices)
        {
            // Throw error if order of entire list is not defined
            if (indices.Count != Nodes.Count)
                throw new ArgumentException("Given index list is a different length to Node list");

            // Swap nodes so new order is the same as that defined in given indices list
            for (int i = 0; i < Nodes.Count; i++)
                Nodes.Swap(i, indices[i]);
        }

        /// <summary>
        /// Sets the order of edges in this graphs collection of edges according to the order of the indices in the given list
        /// </summary>
        public void SetEdgeOrder(List<int> indices)
        {
            // Throw error if order of entire list is not defined
            if (indices.Count != Edges.Count)
                throw new ArgumentException("Given index list is a different length to Edge list");

            // Swap edges so new order is the same as that defined in given indices list
            for (int i = 0; i < Edges.Count; i++)
                Edges.Swap(i, indices[i]);
        }

        /// <summary>
        /// Dissects the graph according to the given clip edge. Parts of the graph that lie outside the edge are removed. Nodes, edges, and 
        /// triangles are created / defined at the intersection of the graph and clipping edge.
        /// </summary>
        public void Clip(GraphEdge clipEdge)
        {
            // Dict that associates edges with their new, clipped version
            Dictionary<GraphEdge, GraphEdge> oldNewEdgeDict = new Dictionary<GraphEdge, GraphEdge>();

            List<GraphTriangle> trianglesCopy = Triangles.ToList();
            foreach (GraphTriangle subjectTriangle in trianglesCopy)
            {
                // Get the indices of all triangle nodes that are 'inside' the given edge
                List<int> insideIndices = subjectTriangle.InsideNodeIndices(clipEdge);

                //    /\
                //   /  \   triangle      ↑
                //  /____\              outside
                // ________ clip edge
                //                      inside
                //                        ↓

                // Case one: NO nodes inside the clip edge, delete the triangle
                if (insideIndices.Count == 0)
                    Remove(subjectTriangle);

                //    ______            
                //  __\____/__ clip edge
                //     \  /
                //      \/
                
                // Case: ONE node inside, two nodes outside, intersection nodes and inside node form a triangle
                else if (insideIndices.Count == 1)
                {
                    // Remember the intersection nodes so they can be triangulated
                    GraphNode[] intersectionNodes = new GraphNode[2];
                    int intersectionIndex = 0;

                    // Get ref to node on inside of clip edge, get outside node iterator
                    GraphNode insideNode = subjectTriangle.Nodes[insideIndices[0]];
                    IEnumerable<GraphNode> outsideNodes = subjectTriangle.Nodes.Where(n => n != insideNode);

                    // Iterate through all nodes in triangle that are outside the clip edge
                    foreach (GraphNode outsideNode in outsideNodes)
                    {
                        // Get ref to edge that has been clipped, check if this edge has already been handled
                        GraphEdge clippedEdge = insideNode.GetEdge(outsideNode);
                        if (oldNewEdgeDict.ContainsKey(clippedEdge))
                        {
                            // Get the new edges intersection node, add to array
                            GraphNode intersectionNode = oldNewEdgeDict[clippedEdge].GetOther(insideNode);
                            intersectionNodes[intersectionIndex++] = intersectionNode;
                        }

                        else
                        {
                            // Calculate the intersection of clip edge and tri-edge
                            Vector2 intersection = MathExtension.KnownIntersection(clipEdge.Nodes[0].Vector, clipEdge.Nodes[1].Vector, insideNode.Vector, outsideNode.Vector);

                            // Create node at intersection, remember node
                            GraphNode intersectionNode = CreateNode(intersection);
                            intersectionNodes[intersectionIndex++] = intersectionNode;

                            // Create an edge from the insideNode to the intersectionNode
                            GraphEdge insideToIntersectionEdge = CreateEdge(insideNode, intersectionNode);

                            // Add old and new edge to dict
                            oldNewEdgeDict.Add(clippedEdge, insideToIntersectionEdge);
                        }
                    }

                    // Create a triangle between the inside node and the two intersection nodes
                    DefineTriangle(insideNode, intersectionNodes[0], intersectionNodes[1]);
                }

                //    /\
                // __/__\__ clip edge
                //  /____\

                // Case: TWO nodes inside, one node outside, intersection nodes and inside nodes form a four sided polygon
                else if (insideIndices.Count == 2)
                {
                    // Remember the intersection nodes so they can be triangulated
                    GraphNode[] intersectionNodes = new GraphNode[2];
                    int index = 0;

                    // Get list of inside nodes by excluding outside node from list
                    int outsideNodeIndex = (insideIndices[0] + insideIndices[1]) - 3;
                    GraphNode outsideNode = subjectTriangle.Nodes[outsideNodeIndex];
                    List<GraphNode> insideNodes = subjectTriangle.Nodes.Where(n => n != outsideNode).ToList();

                    foreach (GraphNode insideNode in insideNodes)
                    {
                        // Get ref to edge that has been clipped, check if this edge has already been handled
                        GraphEdge clippedEdge = insideNode.GetEdge(outsideNode);
                        if (oldNewEdgeDict.ContainsKey(clippedEdge))
                        {
                            // Get the new edges intersection node, add to array
                            GraphNode intersectionNode = oldNewEdgeDict[clippedEdge].GetOther(insideNode);
                            intersectionNodes[index++] = intersectionNode;
                        }

                        else
                        {
                            // Calculate the intersection of clip edge and tri-edge
                            Vector2 intersection = MathExtension.KnownIntersection(clipEdge.Nodes[0].Vector, clipEdge.Nodes[1].Vector, insideNode.Vector, outsideNode.Vector);

                            // Create node at intersection, remember node
                            GraphNode intersectionNode = CreateNode(intersection);
                            intersectionNodes[index++] = intersectionNode;

                            // Create an edge from the insideNode to the intersectionNode
                            GraphEdge insideToIntersectionEdge = CreateEdge(insideNode, intersectionNode);

                            // Add old and new edge refs to dict
                            oldNewEdgeDict.Add(clippedEdge, insideToIntersectionEdge);
                        }
                    }

                    // Add an edge between intersection nodes, creates a four sided polygon
                    GraphEdge intersectionEdge = CreateEdge(intersectionNodes[0], intersectionNodes[1]);

                    // Create an edge that divides the polygon into two triangles
                    GraphEdge bisectingEdge;

                    // We want to create an edge between an intersection node and a non-adjacent inside node
                    if (intersectionNodes[0].HasEdge(insideNodes[0]))               // If there is an edge, the nodes are adjacent   
                        bisectingEdge = CreateEdge(intersectionNodes[1], insideNodes[0]);

                    else
                        bisectingEdge = CreateEdge(intersectionNodes[0], insideNodes[0]);

                    // Create two triangles from four sided polygon
                    DefineTriangle(insideNodes[0], intersectionNodes[0], intersectionNodes[1]);
                    DefineTriangle(insideNodes[1], bisectingEdge.Nodes[0], bisectingEdge.Nodes[1]);
                }
            }
        }
    }
}
