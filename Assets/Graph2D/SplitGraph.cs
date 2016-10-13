using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Graph2D
{
    public class SplitGraph
    {
        public Graph Inside { get; private set; }
        public Graph Outside { get; private set; }

        /// <summary>
        /// Collection of nodes along the split edge. Nodes on the split edge are duplicated with one node occuring in each of the Inside and Outside 
        /// graphs. The 'Key' node is the node in the Outside graph, the 'Value' is the node in the Inside graph.
        /// </summary>
        public Dictionary<GraphNode, GraphNode> SplitNodes { get; set; }

        // All nodes in Inside graph paired with their original node in Outside graph. The 'Key' node is the original node from Outside graph. The
        // 'Value' node is the node in Inside graph.
        private Dictionary<GraphNode, GraphNode> outsideInsidePairs = new Dictionary<GraphNode, GraphNode>();

        // Collection of edges from Outside graph that have been truncated due to the split edge. The 'Key' edge is the original 'pre-truncation'
        // edge, the 'Value' edge is the new truncated version.
        private Dictionary<GraphEdge, GraphEdge> truncatedEdges = new Dictionary<GraphEdge, GraphEdge>();

        private bool split;

        public SplitGraph(Graph original)
        {
            Outside = original;
            Inside = new Graph();
            SplitNodes = new Dictionary<GraphNode, GraphNode>();
        }

        /// <summary>
        /// Splits the graph along the given edge. The original graph is edited to reflect the split. A second graph containing the portion
        /// of the original graph that is inside the given edge is returned.
        /// </summary>
        public SplitGraph Split(Vector2 clipEdgePoint1, Vector2 clipEdgePoint2, float inside)
        {
            // Iterate through every triangle in outside seeing if it has been clipped
            List<GraphTriangle> triangles = Outside.Triangles.ToList();        // Copy collection so can alter original whilst iterating
            foreach (GraphTriangle triangle in triangles)
            {
                // Get nodes, inside, outside, and on the clip edge
                GraphNode[] insideNodes = triangle.Nodes.Where(n => MathExtension.Side(clipEdgePoint1, clipEdgePoint2, n.Vector) == inside).ToArray();
                GraphNode[] onEdgeNodes = triangle.Nodes.Where(n => MathExtension.Side(clipEdgePoint1, clipEdgePoint2, n.Vector) == 0).ToArray();
                GraphNode[] outsideNodes = triangle.Nodes.Where(n => MathExtension.Side(clipEdgePoint1, clipEdgePoint2, n.Vector) == -inside).ToArray();

                // No intersection
                if (outsideNodes.Length == 0)   // Triangle is INSIDE the clip edge
                    CreateTriangleInInsideGraph(triangle.Nodes[0], triangle.Nodes[1], triangle.Nodes[2]); // Cut / Paste triangle to insideGraph:

                else // Clip edge intersects with triangle
                {
                    GraphNode[] intersectionNodes = TruncateTriangle(triangle, insideNodes, onEdgeNodes, outsideNodes, clipEdgePoint1, clipEdgePoint2);

                    /// <summary>
                    /// CASE: (ONE node INSIDE, ONE node ON EDGE, ONE node OUTSIDE)
                    ///        `._____      ↑
                    ///         \`.  /   outside
                    ///  inside  \ `.
                    ///     ↓     \/ `. clip edge                                   
                    /// </summary>
                    if (onEdgeNodes.Length == 1)
                    {
                        // Define new triangle in THIS graph
                        // TODO: Remove old triangle
                        clippedMeshGraph.DefineTriangle(intersectionNodes[0], intersectionNodes[1], outsideNodes.Single());

                        // Add nodes / edges / triangle to split graph
                        GraphNode[] insideTriangleNodes = CreateTriangleInInsideGraph(intersectionNodes[0], intersectionNodes[1], insideNodes.Single());

                        // REMEMBER stitch line node duplicates
                        splitGraph.SplitNodes.Add(intersectionNodes[0], insideTriangleNodes[0]);
                        splitGraph.SplitNodes.Add(intersectionNodes[1], insideTriangleNodes[1]);
                    }

                    /// <summary>
                    /// Case: (ONE node INSIDE, TWO nodes OUTSIDE)
                    ///                        ↑
                    ///           ______    outside     
                    ///         __\____/__ 
                    /// inside     \  /    clip edge          
                    ///    ↓        \/               
                    /// </summary>
                    else if (insideNodes.Length == 1)
                    {
                        // Triangulate the hole in outside graph
                        // TODO: remove old triangle
                        TriangulateHoleInOutsideGraph(intersectionNodes.Union(outsideNodes));

                        // New triangle created in inside graph (inside, intersection, intersection)
                        GraphNode[] insideTriangleNodes = CreateTriangleInInsideGraph(intersectionNodes[0], intersectionNodes[1], insideNodes.Single());

                        // Stitch nodes
                        splitGraph.SplitNodes.Add(intersectionNodes[0], insideTriangleNodes[0]);
                        splitGraph.SplitNodes.Add(intersectionNodes[1], insideTriangleNodes[1]);
                    }

                    /// <summary>
                    /// Case: (TWO nodes INSIDE, ONE node OUTSIDE)
                    ///                      ↑
                    ///            /\     outside
                    ///         __/__\__ 
                    ///  inside  /____\  clip edge
                    ///    ↓
                    /// </summary>
                    else if (outsideNodes.Length == 1)
                    {
                        // Create triangle in THIS graph (outside, intersection, intersection)
                        // TODO: remove triangle
                        clippedMeshGraph.DefineTriangle(intersectionNodes[0], intersectionNodes[1], outsideNodes.Single());

                        // Triangulate hole in insideGraph
                        GraphNode[] insideTriangleNodes = TriangulateHoleInInsideGraph(intersectionNodes, insideNodes);

                        // Stitch nodes
                        splitGraph.SplitNodes.Add(intersectionNodes[0], insideTriangleNodes[0]);
                        splitGraph.SplitNodes.Add(intersectionNodes[1], insideTriangleNodes[1]);
                    }
                }
            }

            // TODO: Remove all nodes from oldNewNodesDict.Keys() from current graph -> maybe dict doesn't contain all necessary nodes
            foreach (GraphNode node in interGraphDuplicateNodes.Keys)
                clippedMeshGraph.Destroy(node);

            return splitGraph;
        }

        public GraphNode GetOrCreateInsideNode(GraphNode outsideNode)
        {
            // If pair dictionary already contains an inside node for the given outside node
            if (outsideInsidePairs.ContainsKey(outsideNode))
                return outsideInsidePairs[outsideNode];                     // Return associated inside node

            GraphNode insideNode = Inside.CreateNode(outsideNode.Vector);   // Create new node at outside node's position
            outsideInsidePairs.Add(outsideNode, insideNode);                // Add node pair to dictionary
            return insideNode;
        }

        private GraphNode[] CreateTriangleInInsideGraph(GraphNode a, GraphNode b, GraphNode c)
        {
            // Get or create nodes
            GraphNode insideA = GetOrCreateInsideNode(a);
            GraphNode insideB = GetOrCreateInsideNode(b);
            GraphNode insideC = GetOrCreateInsideNode(c);

            // Get or create edges
            GraphEdge ab = insideA.HasEdge(insideB) ? insideA.GetEdge(insideB) : Inside.CreateEdge(insideA, insideB);
            GraphEdge ac = insideA.HasEdge(insideC) ? insideA.GetEdge(insideC) : Inside.CreateEdge(insideA, insideC);
            GraphEdge bc = insideB.HasEdge(insideC) ? insideB.GetEdge(insideC) : Inside.CreateEdge(insideB, insideC);

            // Define triangle
            GraphTriangle newTri = Inside.DefineTriangle(ab, ac, bc);

            // Return new triangle nodes as array in SAME ORDER as was given to this method
            return new GraphNode[] { insideA, insideB, insideC };
        }
    }
}
