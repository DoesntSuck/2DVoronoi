using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Graph2D
{
    public static class GraphSplitter
    {
        private static SplitGraph splitGraph;
        private static Graph outsideGraph;
        private static Graph insideGraph;

        // Associates edges with their new, clipped version, so each edge is only truncated once even if it is shared by triangles
        private static Dictionary<GraphEdge, GraphEdge> truncatedEdgeCatalogue;

        // TODO: This comment sucks!
        /// <summary> OUTSIDE - INSIDE NODE ASSOCATIONS:
        /// When two neighbouring triangles are intersected, both require truncation, removal from outside graph, and portions added 
        /// to the outside and inside graphs. When adding the portions to the inside graph, the two nodes these regions share MUST 
        /// NOT be duplicated. By remembering their origin node we can check the original (outside) graph when deciding whether to 
        /// create a node. This also applies when there is no intersection.
        /// </summary>
        private static Dictionary<GraphNode, GraphNode> outsideInsideNodeAssociations;

        /// <summary>
        /// Splits the given mesh in two along the given edge.
        /// </summary>
        public static SplitGraph Split(Graph graph, Vector2 edgePoint1, Vector2 edgePoint2, float insideSide)
        {
            // Graph to contain the portion of the meshGraph inside the given edge
            outsideGraph = graph;
            insideGraph = new Graph();
            splitGraph = new SplitGraph(outsideGraph, insideGraph);

            // Associates edges with their clipped version, so each edge is truncated only once even if it is shared by triangles
            truncatedEdgeCatalogue = new Dictionary<GraphEdge, GraphEdge>();
            outsideInsideNodeAssociations = new Dictionary<GraphNode, GraphNode>();

            // Copy collection so can alter original whilst iterating
            List<GraphTriangle> triangles = outsideGraph.Triangles.ToList();        

            // Iterate through every triangle in outside Graph seeing if it has been clipped
            foreach (GraphTriangle triangle in triangles)
                AssignTriangleToGraph(triangle, edgePoint1, edgePoint2, insideSide);

            // Return inside, outside graphs and the nodes shared between them
            return splitGraph;
        }

        /// <summary>
        /// Splits the given triangle along the given edge if an intersection occurs. In the case of no intersection the triangle is wholly
        /// assigned to either the inside or outside graph.
        /// </summary>
        private static void AssignTriangleToGraph(GraphTriangle triangle, Vector3 clipEdgePoint1, Vector3 clipEdgePoint2, float insideSide)
        {
            // Get nodes, inside, outside, and on the clip edge
            GraphNode[] insideNodes = triangle.Nodes.Where(n => MathExtension.Side(clipEdgePoint1, clipEdgePoint2, n.Vector) == insideSide).ToArray();
            GraphNode[] onEdgeNodes = triangle.Nodes.Where(n => MathExtension.Side(clipEdgePoint1, clipEdgePoint2, n.Vector) == 0).ToArray();
            GraphNode[] outsideNodes = triangle.Nodes.Where(n => MathExtension.Side(clipEdgePoint1, clipEdgePoint2, n.Vector) == -insideSide).ToArray();

            // No intersection - aka Triangle is INSIDE the clip edge - Cut / Paste triangle to insideGraph:
            if (outsideNodes.Length == 0)
                MoveTriangleToInsideGraph(triangle);

            // Also no intersection
            else if (insideNodes.Length == 0) { /* Nothing - triangle already exists in outside graph */ }

            // Clip edge intersects with triangle
            else
            {
                // CASE: (ONE node INSIDE, ONE node ON EDGE, ONE node OUTSIDE)                              
                if (onEdgeNodes.Length == 1)
                    OneInOneOnOneOut(triangle, insideNodes, onEdgeNodes, outsideNodes, clipEdgePoint1, clipEdgePoint2);

                // Case: (ONE node INSIDE, TWO nodes OUTSIDE)
                else if (insideNodes.Length == 1)
                    OneInTwoOut(triangle, insideNodes, onEdgeNodes, outsideNodes, clipEdgePoint1, clipEdgePoint2);

                // Case: (TWO nodes INSIDE, ONE node OUTSIDE)
                else if (outsideNodes.Length == 1)
                    TwoInOneOut(triangle, insideNodes, onEdgeNodes, outsideNodes, clipEdgePoint1, clipEdgePoint2);
            }
        }

        /// <summary> CASE: (ONE node INSIDE, ONE node ON EDGE, ONE node OUTSIDE): 
        /// Handles the case for when the clip edge intersects a node and an edge of the triangle leaving one node inside the edge,
        /// and one node outside the edge. The intersected edge of the triangle must be truncated in the outside graph, the newly 
        /// inserted node along with the intersection node, and the outside node define a new triangle in the outside graph. The other 
        /// side of the given triangle must be created in the inside graph. 
        /// </summary>
        /// <example>
        /// Diagram showing clip edge intersection
        /// <code>
        ///        `._____      ↑
        ///         \`.  /   outside
        ///  inside  \ `.
        ///     ↓     \/ `. clip edge      
        /// </code>
        /// </example>
        private static void OneInOneOnOneOut(GraphTriangle triangle,
                                      GraphNode[] insideNodes, GraphNode[] onEdgeNodes, GraphNode[] outsideNodes,
                                      Vector2 clipEdgePoint1, Vector2 clipEdgePoint2)
        {
            // Get and / or create nodes at the intersection of the clip edge and the given triangle
            GraphNode[] intersectionNodes = TruncateOutsideTriangleEdges(triangle,
                                                                        insideNodes, onEdgeNodes, outsideNodes,
                                                                        clipEdgePoint1, clipEdgePoint2);

            // Define new triangle in THIS graph
            // TODO: Remove old triangle
            outsideGraph.DefineTriangle(intersectionNodes[0], intersectionNodes[1], outsideNodes.Single());

            // Add nodes / edges / triangle to split graph
            GraphNode[] insideTriangleNodes = CreateTriangleInInsideGraph(intersectionNodes[0], intersectionNodes[1], insideNodes.Single());

            // REMEMBER stitch line node duplicates
            splitGraph.AddSplitNode(intersectionNodes[0], insideTriangleNodes[0]);
            splitGraph.AddSplitNode(intersectionNodes[1], insideTriangleNodes[1]);
        }

        /// <summary> CASE: (ONE node INSIDE, TWO nodes OUTSIDE):
        /// Handles the case for when the given clip edge intersects two triangle edges, leaving one node inside and two nodes outside
        /// the edge. Both intersected edges must be truncated in the outside graph, creating two new intersection points. These points
        /// along with the two outside nodes create a for sided polygon that must be trianglulated. The two intersection points along with
        /// the inside node must be added to the inside graph and defined as a triangle.
        /// </summary>
        /// <example>
        /// Diagram showing clip edge intersection
        /// <code>
        ///                        ↑
        ///           ______    outside     
        ///         __\____/__ 
        /// inside     \  /    clip edge          
        ///    ↓        \/               
        /// </code>
        /// </example>
        private static void OneInTwoOut(GraphTriangle triangle,
                                 GraphNode[] insideNodes, GraphNode[] onEdgeNodes, GraphNode[] outsideNodes,
                                 Vector2 clipEdgePoint1, Vector2 clipEdgePoint2)
        {
            // Get and / or create nodes at the intersection of the clip edge and the given triangle
            GraphNode[] intersectionNodes = TruncateOutsideTriangleEdges(triangle,
                                                            insideNodes, onEdgeNodes, outsideNodes,
                                                            clipEdgePoint1, clipEdgePoint2);

            // Triangulate the hole in outside graph
            TriangulateHoleInOutsideGraph(intersectionNodes.Union(outsideNodes));

            // New triangle created in inside graph (inside, intersection, intersection)
            GraphNode[] insideTriangleNodes = CreateTriangleInInsideGraph(intersectionNodes[0], intersectionNodes[1], insideNodes.Single());

            // Stitch nodes
            splitGraph.AddSplitNode(intersectionNodes[0], insideTriangleNodes[0]);
            splitGraph.AddSplitNode(intersectionNodes[1], insideTriangleNodes[1]);
        }

        /// <summary> CASE: (TWO nodes INSIDE, ONE node OUTSIDE): 
        /// Handles the case for when the clip edge intersects two of the triangle's edges, leaving two nodes inside and one node outside
        /// the edge. Both intersected edges must be truncated in the outside graph, creating two new intersection points. These points
        /// along with the outside node are defined as a new triangle. The two intersection points along with the two inside nodes are
        /// added to the inside graph and the resultant four sided polygon must be triangulated.
        /// </summary>
        /// <example>
        /// Diagram showing clip edge intersection
        /// <code>
        ///                      ↑
        ///            /\     outside
        ///         __/__\__ 
        ///  inside  /____\  clip edge
        ///    ↓
        /// </code>
        /// </example>
        private static void TwoInOneOut(GraphTriangle triangle,
                                 GraphNode[] insideNodes, GraphNode[] onEdgeNodes, GraphNode[] outsideNodes,
                                 Vector2 clipEdgePoint1, Vector2 clipEdgePoint2)
        {
            // Get and / or create nodes at the intersection of the clip edge and the given triangle
            GraphNode[] intersectionNodes = TruncateOutsideTriangleEdges(triangle,
                                                                        insideNodes, onEdgeNodes, outsideNodes,
                                                                        clipEdgePoint1, clipEdgePoint2);

            // Create triangle in THIS graph (outside, intersection, intersection)

            // TODO: Error here! No edge between two of the nodes
            outsideGraph.DefineTriangle(intersectionNodes[0], intersectionNodes[1], outsideNodes.Single());

            // Triangulate hole in insideGraph
            GraphNode[] insideTriangleNodes = TriangulateHoleInInsideGraph(intersectionNodes, insideNodes);

            // Stitch nodes
            splitGraph.AddSplitNode(intersectionNodes[0], insideTriangleNodes[0]);
            splitGraph.AddSplitNode(intersectionNodes[1], insideTriangleNodes[1]);
        }

        private static void TriangulateHoleInOutsideGraph(IEnumerable<GraphNode> holeNodes)
        {
            // Get node opposite to first node
            GraphNode oppositeNode = null;
            foreach (GraphNode node in holeNodes.Where(n => n != holeNodes.First()))    // Check all nodes except first node
                if (!holeNodes.First().HasEdge(node)) oppositeNode = node;

            // Create edge between first node and opposite node
            GraphEdge intersectingEdge = outsideGraph.CreateEdge(holeNodes.First(), oppositeNode);

            // Create triangle between intersecting edge and other nodes in enumeration
            foreach (GraphNode node in holeNodes.Where(n => n != holeNodes.First() && n != oppositeNode))   // Check all nodes except first and opposite
                outsideGraph.CreateTriangle(intersectingEdge, node);
        }

        private static GraphNode[] TriangulateHoleInInsideGraph(GraphNode[] intersectionNodes, GraphNode[] insideNodes)
        {
            // Add nodes to inside graph and triangulate the hole
            GraphNode insideA = GetOrCreateInsideNode(intersectionNodes[0]);
            GraphNode insideB = GetOrCreateInsideNode(intersectionNodes[1]);
            GraphNode insideC = GetOrCreateInsideNode(insideNodes[0]);
            GraphNode insideD = GetOrCreateInsideNode(insideNodes[1]);

            // Create edges where necessary
            GraphEdge insideAB = insideGraph.CreateEdge(insideA, insideB);
            GraphEdge insideAC = insideA.HasEdge(insideC) ? insideA.GetEdge(insideC) : insideGraph.CreateEdge(insideA, insideC);
            GraphEdge insideAD = insideA.HasEdge(insideD) ? insideA.GetEdge(insideD) : insideGraph.CreateEdge(insideA, insideD);
            GraphEdge insideBD = insideB.HasEdge(insideD) ? insideB.GetEdge(insideD) : insideGraph.CreateEdge(insideB, insideD);
            GraphEdge insideCD = insideC.HasEdge(insideD) ? insideC.GetEdge(insideD) : insideGraph.CreateEdge(insideC, insideD);

            // Define triangles
            insideGraph.DefineTriangle(insideAB, insideAD, insideBD);
            insideGraph.DefineTriangle(insideAD, insideAC, insideCD);

            // Return nodes from inside graph in the order they were given to this method
            return new GraphNode[] { insideA, insideB, insideC, insideD };
        }

        /// <summary>
        /// Truncates edges of the triangle intersected by the given clipEdge. The outside is updated to reflect the truncation. The 
        /// two points of intersection between the clip edge and triangle are created and returned. An edge is also created between the 
        /// points of intersection. Where a triangle node falls on the clipEdge no node is created, instead the onEdge node is returned 
        /// with the other intersection node.
        /// </summary>
        private static GraphNode[] TruncateOutsideTriangleEdges(GraphTriangle triangle,
                                                        GraphNode[] insideNodes, GraphNode[] onEdgeNodes, GraphNode[] outsideNodes,
                                                        Vector2 clipEdgePoint1, Vector2 clipEdgePoint2)
        {
            // Remember the intersection nodes so they can be triangulated
            GraphNode[] intersectionNodes = new GraphNode[2];
            int index = 0;

            // First intersection node
            if (onEdgeNodes.Length == 1) intersectionNodes[index++] = onEdgeNodes.Single();

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
                        GraphNode intersectionNode = truncatedEdgeCatalogue[clippedEdge].GetOther(outsideNode);
                        intersectionNodes[index++] = intersectionNode;
                    }

                    // EDGE HAS NOT BEEN CLIPPED YET...
                    else
                    {
                        // Calculate the intersection of clip edge and tri-edge
                        Vector2 intersection = MathExtension.KnownIntersection(clipEdgePoint1, clipEdgePoint2, insideNode.Vector, outsideNode.Vector);

                        // Create node at intersection, remember node
                        GraphNode intersectionNode = outsideGraph.CreateNode(intersection);
                        intersectionNodes[index++] = intersectionNode;

                        // Create an edge from the outsideNode to the intersectionNode, add to truncatedEdgeCatalogue
                        GraphEdge outsideToIntersectionEdge = outsideGraph.CreateEdge(outsideNode, intersectionNode);
                        truncatedEdgeCatalogue.Add(clippedEdge, outsideToIntersectionEdge);
                    }
                }
            }

            // Create new edge between intersection points
            outsideGraph.CreateEdge(intersectionNodes[0], intersectionNodes[1]);

            return intersectionNodes;
        }

        private static void MoveTriangleToInsideGraph(GraphTriangle outsideTriangle)
        {
            CreateTriangleInInsideGraph(outsideTriangle.Nodes[0], outsideTriangle.Nodes[1], outsideTriangle.Nodes[2]);
            outsideGraph.Remove(outsideTriangle);
        }

        private static GraphNode[] CreateTriangleInInsideGraph(GraphNode outsideA, GraphNode outsideB, GraphNode outsideC)
        {
            // Get or create nodes
            GraphNode insideA = GetOrCreateInsideNode(outsideA);
            GraphNode insideB = GetOrCreateInsideNode(outsideB);
            GraphNode insideC = GetOrCreateInsideNode(outsideC);

            // Get or create edges
            GraphEdge ab = insideA.HasEdge(insideB) ? insideA.GetEdge(insideB) : insideGraph.CreateEdge(insideA, insideB);
            GraphEdge ac = insideA.HasEdge(insideC) ? insideA.GetEdge(insideC) : insideGraph.CreateEdge(insideA, insideC);
            GraphEdge bc = insideB.HasEdge(insideC) ? insideB.GetEdge(insideC) : insideGraph.CreateEdge(insideB, insideC);

            // Define triangle
            insideGraph.DefineTriangle(ab, ac, bc);

            // Return new triangle nodes as array in SAME ORDER as was given to this method
            return new GraphNode[] { insideA, insideB, insideC };
        }

        private static GraphNode GetOrCreateInsideNode(GraphNode outsideNode)
        {
            if (outsideInsideNodeAssociations.ContainsKey(outsideNode))
                return outsideInsideNodeAssociations[outsideNode];
            else
            {
                GraphNode insideNode = insideGraph.CreateNode(outsideNode.Vector);
                outsideInsideNodeAssociations.Add(outsideNode, insideNode);
                return insideNode;
            }
        }
    }
}
