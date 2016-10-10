﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Graph2D
{
    public class MeshClipper
    {
        public Mesh Mesh { get; private set; }

        private Graph clippedMeshGraph;

        // Graph to contain the portion of the graph inside the given edge
        Graph insideGraph = new Graph();

        // Associates nodes with their duplicate in insideGraph, so that shared nodes between triangles can be maintained
        Dictionary<GraphNode, GraphNode> oldNewNodeDict;

        // Associates edges with their new, clipped version, so each edge is only truncated once even if it is shared by triangles
        Dictionary<GraphEdge, GraphEdge> truncatedEdgeCatalogue;

        // Associates triangles with their duplicate in insideGraph, so that shared edges / nodes between triangles can be maintained
        Dictionary<GraphTriangle, GraphTriangle> oldNewTriDict;

        public MeshClipper(Mesh mesh)
        {
            Mesh = mesh;

            clippedMeshGraph = new Graph(mesh);
        }

        public void Clip(Graph convexClipShape, Vector2 nuclei)
        {
            // Clip once per graph edge
            foreach (GraphEdge clipEdge in convexClipShape.Edges)
            {
                // Which side of edge is counted as being inside?
                float inside = MathExtension.Side(clipEdge.Nodes[0].Vector, clipEdge.Nodes[1].Vector, nuclei);

                // Clip edges that aren't inside of line
                Graph chunk = Split(clipEdge.Nodes[0].Vector, clipEdge.Nodes[1].Vector, inside);
            }
        }

        /// <summary>
        /// Splits the graph along the given edge. The original graph is edited to reflect the split. A second graph containing the portion
        /// of the original graph that is inside the given edge is returned.
        /// </summary>
        public Graph Split(Vector2 clipEdgePoint1, Vector2 clipEdgePoint2, float inside)
        {
            // Graph to contain the portion of the meshGraph inside the given edge
            insideGraph = new Graph();

            oldNewNodeDict = new Dictionary<GraphNode, GraphNode>();         // Associates nodes with duplicates in insideGraph, so that nodes shared between triangles can be maintained
            truncatedEdgeCatalogue = new Dictionary<GraphEdge, GraphEdge>(); // Associates edges with their clipped version, so each edge is truncated only once even if it is shared by triangles
            oldNewTriDict = new Dictionary<GraphTriangle, GraphTriangle>();  // Associates triangles with duplicates in insideGraph, so that shared edges / nodes between triangles can be maintained

            // Iterate through every triangle in Graph seeing if it has been clipped
            List<GraphTriangle> triangles = clippedMeshGraph.Triangles.ToList();        // Copy collection so can alter original whilst iterating
            foreach (GraphTriangle triangle in triangles)
            {
                // Get nodes, inside, outside, and on the clip edge
                GraphNode[] insideNodes = triangle.Nodes.Where(n => MathExtension.Side(clipEdgePoint1, clipEdgePoint2, n.Vector) == inside).ToArray();
                GraphNode[] onEdgeNodes = triangle.Nodes.Where(n => MathExtension.Side(clipEdgePoint1, clipEdgePoint2, n.Vector) == 0).ToArray();
                GraphNode[] outsideNodes = triangle.Nodes.Where(n => MathExtension.Side(clipEdgePoint1, clipEdgePoint2, n.Vector) == -inside).ToArray();

                /// <summary>
                /// CASE: (ZERO nodes OUTSIDE the edge): entire triangle is cut / pasted to inside Graph, even nodes ON the clip edge
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
                    CreateTriangleInInsideGraph(triangle); // Cut / Paste triangle to insideGraph:

                /// <summary>
                /// Case: (ZERO nodes INSIDE the edge): triangle remains in original graph
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
                else if (insideNodes.Length == 0) ; // Do nothing - outside / on edge nodes remain in current graph, no triangles need to be copied to inside graph

                /// <summary>
                /// Case: (ONE node INSIDE, TWO nodes OUTSIDE): intersection nodes and inside node form a triangle
                /// OR    (ONE node INSIDE, ONE node ON EDGE, ONE node OUTSIDE): intersection node, on edge node, and inside node form a triangle
                ///             
                ///    ______                          `._____
                ///  __\____/__ clip edge       OR      \`.  /
                ///     \  /                             \ `.
                ///      \/                               \/ `. clip edge                                   
                /// </summary>
                else if (insideNodes.Length == 1)
                {
                    // Create nodes at intersection of clip edge and triangle edge (or use node if clip edge passes through one)
                    GraphNode[] intersectionNodes = TruncateEdges(triangle, insideNodes, onEdgeNodes, outsideNodes, clipEdgePoint1, clipEdgePoint2);

                    if (outsideNodes.Length == 1)   // Clip edge lies on a node, clip edge creates an inside and an outside triangle
                    {
                        // Define triangle in original graph
                        GraphEdge ab = intersectionNodes[0].GetEdge(intersectionNodes[1]);
                        GraphEdge ac = intersectionNodes[0].GetEdge(outsideNodes[0]);
                        GraphEdge bc = intersectionNodes[1].GetEdge(outsideNodes[0]);

                        // Add triangle to original graph
                        clippedMeshGraph.DefineTriangle(ab, ac, bc);
                    }

                    else    // Two nodes outside: need to triangulate the intersection edge and outside nodes in the outside graph
                    {
                        // Triangulate the hole in outside graph
                        IEnumerable<GraphNode> holeNodes = intersectionNodes.Union(outsideNodes);

                        // Get node opposite to first node
                        GraphNode oppositeNode = null;
                        foreach (GraphNode node in holeNodes.Where(n => n != holeNodes.First()))    // Check all nodes except first node
                            if (!holeNodes.First().HasEdge(node)) oppositeNode = node;

                        // Create edge between first node and opposite node
                        GraphEdge intersectingEdge = clippedMeshGraph.CreateEdge(holeNodes.First(), oppositeNode);

                        // Create triangle between intersecting edge and other nodes in enumeration
                        foreach (GraphNode node in holeNodes.Where(n => n != holeNodes.First() && n != oppositeNode))   // Check all nodes except first and opposite
                            clippedMeshGraph.CreateTriangle(intersectingEdge, node);
                    }

                    // Define triangle in insideGraph, checking if any of the nodes already exist in inside graph

                    // Get node from catalogue if it already exists, otherwise make a new one in insideGraph
                    // TODO: WHEN CREATING NODE ADD TO NEWNODEDICT
                    GraphNode a = GetOrCreateInsideNode(intersectionNodes[0]);
                    GraphNode b = GetOrCreateInsideNode(intersectionNodes[1]);
                    GraphNode c = GetOrCreateInsideNode(insideNodes[0]);

                    // Get connecting edges or create them if they dont exist
                    GraphEdge insideAB = a.HasEdge(b) ? a.GetEdge(b) : insideGraph.CreateEdge(a, b);
                    GraphEdge insideAC = a.HasEdge(c) ? a.GetEdge(c) : insideGraph.CreateEdge(a, c);
                    GraphEdge insideBC = b.HasEdge(c) ? b.GetEdge(c) : insideGraph.CreateEdge(b, c);

                    // Add triangle to inside graph
                    GraphTriangle insideTriangle = clippedMeshGraph.DefineTriangle(insideAB, insideAC, insideBC);
                    oldNewTriDict.Add(triangle, insideTriangle);
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
                    // Create nodes at intersection of clip edge and triangle edge
                    GraphNode[] intersectionNodes = TruncateEdges(triangle, insideNodes, onEdgeNodes, outsideNodes, clipEdgePoint1, clipEdgePoint2);

                    // Define triangle in original graph
                    GraphEdge ab = intersectionNodes[0].GetEdge(intersectionNodes[1]);
                    GraphEdge ac = intersectionNodes[0].GetEdge(outsideNodes[0]);
                    GraphEdge bc = intersectionNodes[1].GetEdge(outsideNodes[0]);

                    // Add triangle to original graph
                    clippedMeshGraph.DefineTriangle(ab, ac, bc);


                    // Add nodes to inside graph and triangulate the hole
                    GraphNode a = GetOrCreateInsideNode(intersectionNodes[0]);
                    GraphNode b = GetOrCreateInsideNode(intersectionNodes[1]);
                    GraphNode c = GetOrCreateInsideNode(insideNodes[0]);
                    GraphNode d = GetOrCreateInsideNode(insideNodes[1]);

                    // Create edges where necessary
                    GraphEdge insideAB = insideGraph.CreateEdge(a, b);
                    GraphEdge insideAC = a.HasEdge(c) ? a.GetEdge(c) : insideGraph.CreateEdge(a, c);
                    GraphEdge insideAD = a.HasEdge(d) ? a.GetEdge(d) : insideGraph.CreateEdge(a, d);
                    GraphEdge insideBD = b.HasEdge(d) ? b.GetEdge(d) : insideGraph.CreateEdge(b, d);
                    GraphEdge insideCD = c.HasEdge(d) ? c.GetEdge(d) : insideGraph.CreateEdge(c, d);

                    // Define triangles
                    insideGraph.DefineTriangle(insideAB, insideAD, insideBD);
                    insideGraph.DefineTriangle(insideAD, insideAC, insideCD);
                }
            }

            // TODO: Remove all nodes from oldNewNodesDict.Keys() from current graph -> maybe dict doesn't contain all necessary nodes
            foreach (GraphNode node in oldNewNodeDict.Keys)
                clippedMeshGraph.Destroy(node);

            return insideGraph;
        }

        private GraphNode GetOrCreateInsideNode(GraphNode clippedMeshGraphNode)
        {
            if (oldNewNodeDict.ContainsKey(clippedMeshGraphNode))
                return oldNewNodeDict[clippedMeshGraphNode];
            else
            {
                GraphNode node = insideGraph.CreateNode(clippedMeshGraphNode.Vector);
                oldNewNodeDict.Add(clippedMeshGraphNode, node);
                return node;
            }
        }

        /// <summary>
        /// Truncates edges of the triangle intersected by the given clipEdge. The clippedGraph is updated to reflect the truncation. The two points of intersection between the clip edge and
        /// triangle are created and returned. An edge is also created between the points of intersection. Where a triangle node falls on the clipEdge no node is created, instead the onEdge
        /// node is returned with the other intersection node.
        /// </summary>
        private GraphNode[] TruncateEdges(GraphTriangle triangle, GraphNode[] insideNodes, GraphNode[] onEdgeNodes, GraphNode[] outsideNodes, Vector2 clipEdgePoint1, Vector2 clipEdgePoint2)
        {
            // Remember the intersection nodes so they can be triangulated
            GraphNode[] intersectionNodes = new GraphNode[2];
            int intersectionNodesIndex = 0;

            // Does one of the triangles nodes fall on the clip edge?
            if (onEdgeNodes.Length == 1) intersectionNodes[intersectionNodesIndex++] = onEdgeNodes[0];   // Index is incremented AFTER adding onEdgeNode to array

            // TRUNCATE EDGES of triangle at their intersection with clip edge (in original graph)
            foreach (GraphNode outsideNode in outsideNodes)     // One or two outsideNodes
            {
                foreach (GraphNode insideNode in insideNodes)   // One or two insideNodes
                {
                    // IF EDGE HAS ALREADY BEEN CLIPPED... (by a different triangle)
                    GraphEdge clippedEdge = insideNode.GetEdge(outsideNode);
                    if (truncatedEdgeCatalogue.ContainsKey(clippedEdge))        // Check catalogue of already clipped edges
                    {
                        // Get the new edges intersection node, add to array
                        GraphNode intersectionNode = truncatedEdgeCatalogue[clippedEdge].GetOther(insideNode);
                        intersectionNodes[intersectionNodesIndex++] = intersectionNode;                 // Index is incremented AFTER adding onEdgeNode to array
                    }

                    else // EDGE HAS NOT BEEN CLIPPED YET...
                    {
                        // Calculate the intersection of clip edge and tri-edge
                        Vector2 intersection = MathExtension.KnownIntersection(clipEdgePoint1, clipEdgePoint2, insideNode.Vector, outsideNode.Vector);

                        // Create node at intersection, remember node
                        GraphNode intersectionNode = clippedMeshGraph.CreateNode(intersection);
                        intersectionNodes[intersectionNodesIndex++] = intersectionNode;

                        // Create an edge from the insideNode to the intersectionNode, add to truncatedEDgeCatalogue
                        GraphEdge insideToIntersectionEdge = clippedMeshGraph.CreateEdge(insideNode, intersectionNode);
                        truncatedEdgeCatalogue.Add(clippedEdge, insideToIntersectionEdge);
                    }
                }
            }

            // Create new edge between intersection points
            clippedMeshGraph.CreateEdge(intersectionNodes[0], intersectionNodes[1]);

            return intersectionNodes;
        }

        // Need to be able to copy ONLY given nodes
        private void CreateTriangleInInsideGraph(GraphTriangle triangle)
        {
            // Get all triangles in original graph that:
                // Share a node with THIS triangle, 
                // Also have a copy in the new graph
            IEnumerable<GraphTriangle> neighbours = oldNewTriDict.Keys.Where(t => t.SharesNode(triangle));

            // Create list of all nodes from neighbouring triangles
            List<GraphNode> neighbourNodes = new List<GraphNode>();
            foreach (GraphTriangle neighbour in neighbours)
                neighbourNodes.AddRange(neighbour.Nodes);

            // Shorten list to those nodes shared by this triangle and neighbouring triangles (in original graph)
            List<GraphNode> sharedNodes = triangle.Nodes.Intersect(neighbourNodes).ToList();
            List<GraphNode> uniqueNodes = triangle.Nodes.Except(sharedNodes).ToList();          // Nodes not shared by a triangle from inside graph

            // Convert shared nodes to a list of nodes in INSIDE GRAPH
            List<GraphNode> insideGraphSharedNodes = sharedNodes.Select(n => oldNewNodeDict[n]).ToList();
            if (insideGraphSharedNodes.Count == 0)
            {
                // Create nodes in inside graph
                GraphNode a = insideGraph.CreateNode(triangle.Nodes[0].Vector);
                GraphNode b = insideGraph.CreateNode(triangle.Nodes[1].Vector);
                GraphNode c = insideGraph.CreateNode(triangle.Nodes[2].Vector);

                // Remember association to nodes in current graph
                oldNewNodeDict.Add(triangle.Nodes[0], a);
                oldNewNodeDict.Add(triangle.Nodes[1], b);
                oldNewNodeDict.Add(triangle.Nodes[2], c);

                // Create edges in inside graph
                GraphEdge ab = insideGraph.CreateEdge(a, b);
                GraphEdge ac = insideGraph.CreateEdge(a, c);
                GraphEdge bc = insideGraph.CreateEdge(b, c);

                // Get ref to associated edges in current graph
                GraphEdge oldAB = triangle.Nodes[0].GetEdge(triangle.Nodes[1]);
                GraphEdge oldAC = triangle.Nodes[0].GetEdge(triangle.Nodes[2]);
                GraphEdge oldBC = triangle.Nodes[1].GetEdge(triangle.Nodes[2]);

                // Create triangle in inside graph
                GraphTriangle insideGraphTriangle = insideGraph.DefineTriangle(ab, ac, bc);

                // Remember association to triangle in current graph
                oldNewTriDict.Add(triangle, insideGraphTriangle);
            }

            else if (insideGraphSharedNodes.Count == 1)
            {
                GraphNode a = insideGraphSharedNodes[0];    // Get ref to already existing duplicate node
                GraphNode b = uniqueNodes[0];               // Create duplicate of unique nodes
                GraphNode c = uniqueNodes[1];

                // Remember association to nodes in current graph
                oldNewNodeDict.Add(uniqueNodes[0], b);
                oldNewNodeDict.Add(uniqueNodes[1], c);

                // Create edges in inside graph
                GraphEdge ab = insideGraph.CreateEdge(a, b);
                GraphEdge ac = insideGraph.CreateEdge(a, c);
                GraphEdge bc = insideGraph.CreateEdge(b, c);

                // Create triangle in inside graph
                GraphTriangle insideGraphTriangle = insideGraph.DefineTriangle(ab, ac, bc);

                // Remember association to triangle in current graph
                oldNewTriDict.Add(triangle, insideGraphTriangle);
            }

            else if (insideGraphSharedNodes.Count == 2)
            {
                GraphNode a = insideGraphSharedNodes[0];    // Get ref to already existing duplicate nodes
                GraphNode b = insideGraphSharedNodes[1];
                GraphNode c = uniqueNodes[0];               // Create duplicate of unique node

                // Remember association to unique node in current graph
                oldNewNodeDict.Add(uniqueNodes[1], c);

                // Create necessary edges in inside graph
                GraphEdge ab = insideGraphSharedNodes[0].GetEdge(insideGraphSharedNodes[1]);    // Get ref to already existing duplicate edge
                GraphEdge ac = insideGraph.CreateEdge(a, c);                                    // Create duplicate edges
                GraphEdge bc = insideGraph.CreateEdge(b, c);

                // Create triangle in inside graph
                GraphTriangle insideGraphTriangle = insideGraph.DefineTriangle(ab, ac, bc);

                // Remember association to triangle in current graph
                oldNewTriDict.Add(triangle, insideGraphTriangle);
            }

            else
            {
                // Define triangle using shared nodes ONLY if a triangle has not already been defined here
                if (!insideGraph.ContainsTriangle(insideGraphSharedNodes[0], insideGraphSharedNodes[1], insideGraphSharedNodes[2]))
                {
                    // Get ref to already existing duplicate edges
                    GraphEdge ab = insideGraphSharedNodes[0].GetEdge(insideGraphSharedNodes[1]);
                    GraphEdge ac = insideGraphSharedNodes[0].GetEdge(insideGraphSharedNodes[2]);
                    GraphEdge bc = insideGraphSharedNodes[1].GetEdge(insideGraphSharedNodes[2]);

                    // Define triangle using already duplicated edges
                    GraphTriangle insideGraphTriangle = insideGraph.DefineTriangle(ab, ac, bc);

                    // Remember association to triangle in current graph
                    oldNewTriDict.Add(triangle, insideGraphTriangle);
                }
            }
        }



        // Iterate through all triangles
        // Get number of:
        // Inside nodes
        // Outside nodes
        // On edge nodes
        // If (Outside nodes == 0)
        // Copy tri to inside graph
        // else if(Inside nodes == 0)
        // Triangle is marked for deletion BUT, check each ON EDGE node to see if it is shared by any1... DONT DELETE SHARED ON EDGE NODES
        // else if (Inside nodes == 2)
        // 4-sided polygon method: triangulate
        // else
        // 2 nodes outside method: create triangle with intersection edge
        // if (On edge nodes == 1)
        // the on edge node becaomes one of the nodes on the intersection edge

        private void ClipGraphTriangles(Vector2 clipEdgePoint1, Vector2 clipEdgePoint2, float inside)
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
