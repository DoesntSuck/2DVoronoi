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

        public bool Closed()
        {
            List<GraphNode> outsideNodes = OutsideNodes();

            foreach (GraphNode node in outsideNodes)
            {
                if (node.Edges.Count < 2)
                    return false;
            }

            return true;
        }

        // Iterate through all triangles
        // Get number of:
            // Inside nodes
            // Outside nodes
            // On edge nodes
        // If (Outside nodes == 0)
            // Triangle stays, carry on...
        // else if(Inside nodes == 0)
            // Triangle is marked for deletion BUT, check each ON EDGE node to see if it is shared by any1... DONT DELETE SHARED ON EDGE NODES
        // else if (Inside nodes == 2)
            // 4-sided polygon method: triangulate
        // else
            // 2 nodes outside method: create triangle with intersection edge
            // if (On edge nodes == 1)
                // the on edge node becaomes one of the nodes on the intersection edge

        public void Clip(Vector2 edgePoint1, Vector2 edgePoint2, float inside)
        {
            // Dict that associates edges with their new, clipped version
            Dictionary<GraphEdge, GraphEdge> oldNewEdgeDict = new Dictionary<GraphEdge, GraphEdge>();
            List<GraphNode> deadNodes = new List<GraphNode>();

            // Iterate through every triangle in Graph seeing if it has been clipped
            List<GraphTriangle> trianglesCopy = Triangles.ToList();
            foreach (GraphTriangle subjectTriangle in trianglesCopy)
            {
                // Get nodes, inside, outside, and on the clip edge
                GraphNode[] insideNodes = subjectTriangle.SameSideNodes(edgePoint1, edgePoint2, inside).ToArray();
                GraphNode[] onEdgeNodes = subjectTriangle.OnEdgeNodes(edgePoint1, edgePoint2).ToArray();
                GraphNode[] outsideNodes = subjectTriangle.OpposideSideNodes(edgePoint1, edgePoint2, inside).ToArray();

                /// <summary>
                /// CASE: (ZERO nodes OUTSIDE the triangle): entire triangle is kept, even nodes ON the clip edge
                ///                                ↑
                ///                             outside
                ///  ________ clip edge     
                ///                         __________ clip edge      __________ clip edge
                ///     /\                    \    /                      /\
                ///    /  \        OR          \  /          OR          /  \
                ///   /____\                    \/                      /____\
                ///         
                ///                             inside
                ///                                ↓
                /// </summary>
                if (outsideNodes.Length == 0)
                {
                    /* Nothing */
                }

                /// <summary>
                /// Case: (ZERO nodes INSIDE the triangle): triangle gets deleted, nodes ON the edge are saved if in use by another triangle
                ///                               ↑
                ///                            outside
                ///
                ///    /\                        /\                        ______     
                ///   /  \            OR        /  \             OR        \    /
                ///  /____\                  __/____\__ clip edge           \  /
                /// ________ clip edge                                   ____\/____ clip edge
                ///                            inside                         
                ///                               ↓
                /// </summary>
                else if (insideNodes.Length == 0)
                {
                    // Outside nodes get deleted
                    foreach (GraphNode outsideNode in outsideNodes)
                        deadNodes.Add(outsideNode);

                    // On edge nodes are deleted if not in use by another triangle
                    foreach (GraphNode onEdgeNode in onEdgeNodes)
                    {
                        if (onEdgeNode.Triangles.Count == 1)
                            deadNodes.Add(onEdgeNode);
                    }
                }

                /// <summary>
                /// Case: (ONE node INSIDE, TWO nodes OUTSIDE): intersection nodes and inside node form a triangle
                /// OR    (ONE node INSIDE, ONE node ON EDGE, ONE node OUTSIDE): intersection node, on edge node, and inside node form a triangle
                ///    ______                          `._____
                ///  __\____/__ clip edge       OR      \`.  /
                ///     \  /                             \ `.
                ///      \/                               \/ `. clip edge                                   
                /// </summary>
                else if (insideNodes.Length == 1)
                {
                    // Rename ref to ONLY inside node
                    GraphNode insideNode = insideNodes[0];

                    // Remember the intersection nodes so they can be triangulated
                    GraphNode[] intersectionNodes = new GraphNode[2];
                    int intersectionNodesIndex = 0;
                    if (onEdgeNodes.Length == 1)
                        intersectionNodes[intersectionNodesIndex++] = onEdgeNodes[0];   // Index is incremented AFTER adding onEdgeNode to array

                    // Iterate through all nodes in triangle that are outside the clip edge
                    foreach (GraphNode outsideNode in outsideNodes)
                    {
                        // Mark outside node for later removal
                        deadNodes.Add(outsideNode);         

                        // IF EDGE HAS ALREADY BEEN CLIPPED... (by a different triangle)
                        GraphEdge clippedEdge = insideNode.GetEdge(outsideNode);
                        if (oldNewEdgeDict.ContainsKey(clippedEdge))
                        {
                            // Get the new edges intersection node, add to array
                            GraphNode intersectionNode = oldNewEdgeDict[clippedEdge].GetOther(insideNode);
                            intersectionNodes[intersectionNodesIndex++] = intersectionNode;
                        }

                        else // EDGE HAS NOT BEEN CLIPPED YET...
                        {
                            // Calculate the intersection of clip edge and tri-edge
                            Vector2 intersection = MathExtension.KnownIntersection(edgePoint1, edgePoint2, insideNode.Vector, outsideNode.Vector);

                            // Create node at intersection, remember node
                            GraphNode intersectionNode = CreateNode(intersection);
                            intersectionNodes[intersectionNodesIndex++] = intersectionNode;

                            // Create an edge from the insideNode to the intersectionNode, add to oneNewEdgeDict
                            GraphEdge insideToIntersectionEdge = CreateEdge(insideNode, intersectionNode);
                            oldNewEdgeDict.Add(clippedEdge, insideToIntersectionEdge);
                        }
                    }

                    // Get edges from which to define triangle
                    GraphEdge ab = insideNode.GetEdge(intersectionNodes[0]);
                    GraphEdge ac = insideNode.GetEdge(intersectionNodes[1]);
                    GraphEdge bc = CreateEdge(intersectionNodes[0], intersectionNodes[1]);

                    // Create a triangle between the inside node and the two intersection nodes
                    GraphTriangle newTriangle = DefineTriangle(ab, ac, bc);
                    newTriangle.OrderNodes();
                }

                /// <summary>
                /// Case: (TWO nodes INSIDE, ONE node OUTSIDE): intersection nodes and inside nodes form a four sided polygon; triangulate the polygon
                ///    ↑
                ///  outside
                ///
                ///    /\
                /// __/__\__ clip edge
                ///  /____\
                ///
                ///  inside
                ///     ↓
                /// </summary>
                else
                {
                    // Rename ref to ONLY outside node
                    GraphNode outsideNode = outsideNodes[0];
                    deadNodes.Add(outsideNode);                 // Mark outside node for later removal

                    // Remember the intersection nodes so they can be triangulated
                    GraphNode[] intersectionNodes = new GraphNode[2];
                    int intersectionNodeIndex = 0;

                    foreach (GraphNode insideNode in insideNodes)
                    {
                        // IF EDGE HAS ALREADY BEEN CLIPPED... (by a different triangle)
                        GraphEdge clippedEdge = insideNode.GetEdge(outsideNode);
                        if (oldNewEdgeDict.ContainsKey(clippedEdge))
                        {
                            // Get the new edges intersection node, add to array
                            GraphNode intersectionNode = oldNewEdgeDict[clippedEdge].GetOther(insideNode);
                            intersectionNodes[intersectionNodeIndex++] = intersectionNode;
                        }

                        else // EDGE HAS NOT BEEN CLIPPED YET...
                        {
                            // Calculate the intersection of clip edge and tri-edge
                            Vector2 intersection = MathExtension.KnownIntersection(edgePoint1, edgePoint2, insideNode.Vector, outsideNode.Vector);

                            // Create node at intersection, remember node
                            GraphNode intersectionNode = CreateNode(intersection);
                            intersectionNodes[intersectionNodeIndex++] = intersectionNode;

                            // Create an edge from the insideNode to the intersectionNode add to oldNewEdgeDict
                            GraphEdge insideToIntersectionEdge = CreateEdge(insideNode, intersectionNode);
                            oldNewEdgeDict.Add(clippedEdge, insideToIntersectionEdge);
                        }
                    }

                    /// <summary>
                    /// Cropping the triangle's edges has created a 4 sided hole: need to triangulate the hole.
                    /// 
                    ///                                deleted node
                    ///                                     /\
                    ///                                 ___/__\___ clip edge
                    ///                                   /    \
                    ///                                  / hole \
                    ///                                 /________\
                    /// 
                    /// The hole is triangulated by creating an edge that bisects the hole from one intersection node to the non-adjacent inside node,
                    /// as in one of the diagrams below. The following calculations are used to correctly identify edges.
                    /// 
                    ///                  .   /\                               /\   ,'
                    ///               ____`./__\____     clip edge      _____/__\,'____ 
                    ///                    /`.  \                           /  ,'\
                    ///     insideSide    /   `. \    intersectionSide     / ,'   \    insideSide
                    ///                  /______`.\                       /,'______\
                    ///                           `.                    ,'
                    ///                             `. bisecting edge ,'
                    /// </summary>   

                    // Add an edge between intersection nodes, creates a four sided polygon
                    GraphEdge intersectionEdge = CreateEdge(intersectionNodes[0], intersectionNodes[1]);
                    GraphEdge insideEdge = insideNodes[0].GetEdge(insideNodes[1]);      // Inside edge already exists

                    // Edge that wil divide the hole into two triangles
                    GraphEdge bisectingEdge;

                    // Edge references for identifying the above edges
                    GraphEdge intersectionSideEdge;
                    GraphEdge insideSideEdge;

                    // IF THERE IS AN EDGE, THE NODES ARE ADJACENT... 
                    if (intersectionNodes[0].HasEdge(insideNodes[0]))                
                    {
                        // Bisecting edge connects non-adjacent edges
                        bisectingEdge = CreateEdge(intersectionNodes[0], insideNodes[1]);

                        // Identifying the other edges
                        intersectionSideEdge = intersectionNodes[1].GetEdge(insideNodes[1]);
                        insideSideEdge = intersectionNodes[0].GetEdge(insideNodes[0]);
                    }

                    else // NO EDGE, THE NODES ARE NOT ADJACENT...
                    {
                        // Bisecting edge connects non-adjacent edges
                        bisectingEdge = CreateEdge(intersectionNodes[0], insideNodes[0]);

                        // Identifying the other edges
                        intersectionSideEdge = intersectionNodes[1].GetEdge(insideNodes[0]);
                        insideSideEdge = intersectionNodes[0].GetEdge(insideNodes[1]);
                    }

                    // Create two triangles from four sided hole
                    GraphTriangle intersectionSideTriangle = DefineTriangle(bisectingEdge, intersectionEdge, intersectionSideEdge);
                    intersectionSideTriangle.OrderNodes();

                    GraphTriangle insideSideTriangle = DefineTriangle(insideSideEdge, insideEdge, bisectingEdge);
                    insideSideTriangle.OrderNodes();
                }
            }

            // Remove nodes outside the crop edge
            foreach (GraphNode deadNode in deadNodes)
                Destroy(deadNode);
        }
    }
}
