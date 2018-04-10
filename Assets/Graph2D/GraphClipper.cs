using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Graph2D
{
    /// <summary>
    /// Provides functions for cutting a graph along an edge.
    /// </summary>
    public static class GraphClipper
    {
        /// <summary>
        /// Clips a graph once for each pair of points in the edgePoints collection.
        /// </summary>
        /// <param name="graph">The graph to clip.</param>
        /// <param name="edgePoints">Edge points defining a convex polygon. GraphNodes
        /// outside of this shape are clipped</param>
        /// <param name="nuclei">A point that defines the inside of the convex
        /// polygonal clipping shape</param>
        public static void Clip(Graph graph, IList<Vector2> edgePoints, Vector2 nuclei)
        {
            // Edge points are assumed to come in pairs, the points of
            // one pair don't necessarily touch the points of the next
            for (int i = 0; i < edgePoints.Count; i += 2)
            {
                Vector2 a = edgePoints[i];
                Vector2 b = edgePoints[i + 1];

                // Which side of edge is counted as being inside?
                float inside = Geometry.Side(a, b, nuclei);

                // Clip edges that aren't inside of line
                Clip(graph, a, b, inside);
            }
        }

        /// <summary>
        /// Clips the graph according to the given edge. Parts of the graph
        /// outside of the edge are removed. Additional, nodes are added on
        /// the clipping edge to repair the graph. The clipping edge is 
        /// considered to be infinitely long.
        /// </summary>
        /// <param name="graph">The graph to clip</param>
        /// <param name="edgePoint1">First point of the clipping edge</param>
        /// <param name="edgePoint2">Second point of the clipping edge</param>
        /// <param name="inside">Determines side of the clipping edge is
        /// considered 'inside'. GraphNodes outside the line are removed</param>
        public static void Clip(Graph graph, Vector2 edgePoint1, Vector2 edgePoint2, float inside)
        {
            /// <summary>
            /// Iterate through all triangles, get number of:
            ///     Inside nodes
            ///     Outside nodes
            ///     On edge nodes
            /// If (Outside nodes == 0)
            ///     Triangle stays, carry on...
            /// else if(Inside nodes == 0)
            ///     Triangle is marked for deletion BUT: 
            ///     check each ON EDGE node to see if it is shared by any1... 
            ///     DONT DELETE SHARED ON EDGE NODES
            /// else if (Inside nodes == 2)
            ///     4-sided polygon method: triangulate
            /// else
            ///     2 nodes outside method: create triangle with intersection edge
            /// if (On edge nodes == 1)
            ///     the on edge node becaomes one of the nodes on the intersection edge
            /// </summary>

            // Dict that associates edges with their new, clipped version
            Dictionary<GraphEdge, GraphEdge> oldNewEdgeDict = new Dictionary<GraphEdge, GraphEdge>();
            List<GraphNode> deadNodes = new List<GraphNode>();

            // Iterate through every triangle in Graph seeing if it has been clipped
            List<GraphTriangle> trianglesCopy = graph.Triangles.ToList();
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
                            Vector2 intersection = Geometry.KnownIntersection(edgePoint1, edgePoint2, insideNode.Vector, outsideNode.Vector);

                            // Create node at intersection, remember node
                            GraphNode intersectionNode = graph.CreateNode(intersection);
                            intersectionNodes[intersectionNodesIndex++] = intersectionNode;

                            // Create an edge from the insideNode to the intersectionNode, add to oneNewEdgeDict
                            GraphEdge insideToIntersectionEdge = graph.CreateEdge(insideNode, intersectionNode);
                            oldNewEdgeDict.Add(clippedEdge, insideToIntersectionEdge);
                        }
                    }

                    // Get edges from which to define triangle
                    GraphEdge ab = insideNode.GetEdge(intersectionNodes[0]);
                    GraphEdge ac = insideNode.GetEdge(intersectionNodes[1]);
                    GraphEdge bc = graph.CreateEdge(intersectionNodes[0], intersectionNodes[1]);

                    // Create a triangle between the inside node and the two intersection nodes
                    GraphTriangle newTriangle = graph.DefineTriangle(ab, ac, bc);
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
                            Vector2 intersection = Geometry.KnownIntersection(edgePoint1, edgePoint2, insideNode.Vector, outsideNode.Vector);

                            // Create node at intersection, remember node
                            GraphNode intersectionNode = graph.CreateNode(intersection);
                            intersectionNodes[intersectionNodeIndex++] = intersectionNode;

                            // Create an edge from the insideNode to the intersectionNode add to oldNewEdgeDict
                            GraphEdge insideToIntersectionEdge = graph.CreateEdge(insideNode, intersectionNode);
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
                    GraphEdge intersectionEdge = graph.CreateEdge(intersectionNodes[0], intersectionNodes[1]);
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
                        bisectingEdge = graph.CreateEdge(intersectionNodes[0], insideNodes[1]);

                        // Identifying the other edges
                        intersectionSideEdge = intersectionNodes[1].GetEdge(insideNodes[1]);
                        insideSideEdge = intersectionNodes[0].GetEdge(insideNodes[0]);
                    }

                    else // NO EDGE, THE NODES ARE NOT ADJACENT...
                    {
                        // Bisecting edge connects non-adjacent edges
                        bisectingEdge = graph.CreateEdge(intersectionNodes[0], insideNodes[0]);

                        // Identifying the other edges
                        intersectionSideEdge = intersectionNodes[1].GetEdge(insideNodes[0]);
                        insideSideEdge = intersectionNodes[0].GetEdge(insideNodes[1]);
                    }

                    // Create two triangles from four sided hole
                    GraphTriangle intersectionSideTriangle = graph.DefineTriangle(bisectingEdge, intersectionEdge, intersectionSideEdge);
                    intersectionSideTriangle.OrderNodes();

                    GraphTriangle insideSideTriangle = graph.DefineTriangle(insideSideEdge, insideEdge, bisectingEdge);
                    insideSideTriangle.OrderNodes();
                }
            }

            // Remove nodes outside the crop edge
            foreach (GraphNode deadNode in deadNodes)
                graph.Destroy(deadNode);
        }
    }
}

