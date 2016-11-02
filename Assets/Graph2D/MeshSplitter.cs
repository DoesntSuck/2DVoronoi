using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Graph2D
{
    public class MeshSplitter
    {
        private SplitGraph splitGraph;
        private Graph outsideGraph;
        private Graph insideGraph;

        // Associates edges with their new, clipped version, so each edge is only truncated once even if it is shared by triangles
        private Dictionary<GraphEdge, GraphEdge> truncatedEdgeCatalogue; 

        /// <summary>
        /// OUTSIDE - INSIDE NODE ASSOCATIONS:
        /// Both triangles require truncation, removal from outside graph, and portions added to the outside and inside graphs. When adding
        /// the a and b portions to the inside graph, the two nodes these regions share MUST NOT be duplicated. By remembering their origin
        /// node we can check the original (outside) graph when deciding whether to create a node. This also applies when there is no 
        /// intersection.
        /// 
        ///         inside
        ///        _______
        ///      /\      /
        ///     /a \  b /
        /// ___/____\__/____ clip edge
        ///   /______\/
        /// 
        ///         outside
        /// </summary>

        // Associates nodes with their new, duplicated verion in inside graph so that shared nodes between triangles can be maintained
        private Dictionary<GraphNode, GraphNode> outsideInsideNodeAssociations;

        /// <summary>
        /// Splits the given mesh in two along the given edge.
        /// </summary>
        public SplitGraph Split(Mesh mesh, out Graph outside, out Graph inside, Vector2 edgePoint1, Vector2 edgePoint2, float insideSide)
        {
            // Graph to contain the portion of the meshGraph inside the given edge
            outside = new Graph(mesh);
            inside = new Graph();
            splitGraph = new SplitGraph(outside, inside);

            // Associates edges with their clipped version, so each edge is truncated only once even if it is shared by triangles
            truncatedEdgeCatalogue = new Dictionary<GraphEdge, GraphEdge>();
            outsideInsideNodeAssociations = new Dictionary<GraphNode, GraphNode>();

            // Copy collection so can alter original whilst iterating
            List<GraphTriangle> triangles = outside.Triangles.ToList();        

            // Iterate through every triangle in outside Graph seeing if it has been clipped
            foreach (GraphTriangle triangle in triangles)
                SplitTriangle(triangle, edgePoint1, edgePoint2, insideSide);

            // Return inside, outside graphs and the nodes shared between them
            return splitGraph;
        }

        /// <summary>
        /// Splits the given triangle along the given edge if an intersection occurs. In the case of no intersection the triangle is wholly
        /// assigned to either the inside or outside graph.
        /// </summary>
        private void SplitTriangle(GraphTriangle triangle, Vector3 clipEdgePoint1, Vector3 clipEdgePoint2, float insideSide)
        {
            // Get nodes, inside, outside, and on the clip edge
            GraphNode[] insideNodes = triangle.Nodes.Where(n => MathExtension.Side(clipEdgePoint1, clipEdgePoint2, n.Vector) == insideSide).ToArray();
            GraphNode[] onEdgeNodes = triangle.Nodes.Where(n => MathExtension.Side(clipEdgePoint1, clipEdgePoint2, n.Vector) == 0).ToArray();
            GraphNode[] outsideNodes = triangle.Nodes.Where(n => MathExtension.Side(clipEdgePoint1, clipEdgePoint2, n.Vector) == -insideSide).ToArray();

            // No intersection - aka Triangle is INSIDE the clip edge - Cut / Paste triangle to insideGraph:
            if (outsideNodes.Length == 0)
            {
                CreateTriangleInInsideGraph(triangle.Nodes[0], triangle.Nodes[1], triangle.Nodes[2]);
                outsideGraph.Triangles.Remove(triangle);
            }

            // Also no intersection
            else if (insideNodes.Length == 0) { /* Nothing - triangle already exists in outside graph*/ }

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
                    outsideGraph.DefineTriangle(intersectionNodes[0], intersectionNodes[1], outsideNodes.Single());

                    // Add nodes / edges / triangle to split graph
                    GraphNode[] insideTriangleNodes = CreateTriangleInInsideGraph(intersectionNodes[0], intersectionNodes[1], insideNodes.Single());

                    // REMEMBER stitch line node duplicates
                    chunks.Last().AddSplitNode(intersectionNodes[0], insideTriangleNodes[0]);
                    chunks.Last().AddSplitNode(intersectionNodes[1], insideTriangleNodes[1]);
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
                    chunks.Last().AddSplitNode(intersectionNodes[0], insideTriangleNodes[0]);
                    chunks.Last().AddSplitNode(intersectionNodes[1], insideTriangleNodes[1]);
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

                    // TODO: Error here! No edge between two of the nodes
                    outsideGraph.DefineTriangle(intersectionNodes[0], intersectionNodes[1], outsideNodes.Single());

                    // Triangulate hole in insideGraph
                    GraphNode[] insideTriangleNodes = TriangulateHoleInInsideGraph(intersectionNodes, insideNodes);

                    // Stitch nodes
                    chunks.Last().AddSplitNode(intersectionNodes[0], insideTriangleNodes[0]);
                    chunks.Last().AddSplitNode(intersectionNodes[1], insideTriangleNodes[1]);
                }
            }
        }

        private GraphNode[] CreateTriangleInInsideGraph(GraphNode a, GraphNode b, GraphNode c)
        {
            // Get or create nodes
            GraphNode insideA = GetOrCreateInsideNode(a);
            GraphNode insideB = GetOrCreateInsideNode(b);
            GraphNode insideC = GetOrCreateInsideNode(c);

            // Get or create edges
            GraphEdge ab = insideA.HasEdge(insideB) ? insideA.GetEdge(insideB) : insideGraph.CreateEdge(insideA, insideB);
            GraphEdge ac = insideA.HasEdge(insideC) ? insideA.GetEdge(insideC) : insideGraph.CreateEdge(insideA, insideC);
            GraphEdge bc = insideB.HasEdge(insideC) ? insideB.GetEdge(insideC) : insideGraph.CreateEdge(insideB, insideC);

            // Define triangle
            GraphTriangle newTri = insideGraph.DefineTriangle(ab, ac, bc);

            // Return new triangle nodes as array in SAME ORDER as was given to this method
            return new GraphNode[] { insideA, insideB, insideC };
        }

        private GraphNode GetOrCreateInsideNode(GraphNode outsideNode)
        {
            if (splitGraph.SplitNodes.ContainsKey(outsideNode))
                return splitGraph.SplitNodes[outsideNode];
            else
            {
                GraphNode insideNode = insideGraph.CreateNode(outsideNode.Vector);
                outsideInsideNodeAssociations.Add(outsideNode, insideNode);
                return insideNode;
            }
        }
    }
}
