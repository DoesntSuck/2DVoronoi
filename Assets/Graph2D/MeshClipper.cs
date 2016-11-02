using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Graph2D
{
    public class MeshClipper
    {
        private Graph outside;
        private Graph inside;

        List<SplitGraph> chunks;

        private Dictionary<GraphNode, GraphNode> interGraphDuplicateNodes; // Associates nodes with their duplicate in insideGraph, so that shared nodes between triangles can be maintained
        private Dictionary<GraphEdge, GraphEdge> truncatedEdgeCatalogue; // Associates edges with their new, clipped version, so each edge is only truncated once even if it is shared by triangles

        public MeshClipper(Mesh mesh)
        {
            outside = new Graph(mesh);
            chunks = new List<SplitGraph>();
        }

        public Graph Clip(Graph convexClipShape, Vector2 nuclei)
        {
            // Clip once per graph edge
            foreach (GraphEdge clipEdge in convexClipShape.Edges)
            {
                // Which side of edge is counted as being inside?
                float insideSide = MathExtension.Side(clipEdge.Nodes[0].Vector, clipEdge.Nodes[1].Vector, nuclei);

                // Clip edges that aren't inside of line
                Split(clipEdge.Nodes[0].Vector, clipEdge.Nodes[1].Vector, insideSide);
            }

            // Remove the last chunk: its the inside chunk
            Graph inside = chunks.Last().Inside;
            chunks.RemoveAt(chunks.Count - 1);
            Stitch();

            return inside;
        }

        private void Stitch()
        {
            foreach (SplitGraph splitGraph in chunks.Reverse<SplitGraph>())
                splitGraph.Outside.Stitch(splitGraph.Inside, splitGraph.SplitNodes);
        }

        /// <summary>
        /// Splits the graph along the given edge. The original graph is edited to reflect the split. A second graph containing the portion
        /// of the original graph that is inside the given edge is returned.
        /// </summary>
        private void Split(Vector2 clipEdgePoint1, Vector2 clipEdgePoint2, float insideSide)
        {
            // Graph to contain the portion of the meshGraph inside the given edge
            inside = new Graph();
            chunks.Add(new SplitGraph(outside, inside));

            interGraphDuplicateNodes = new Dictionary<GraphNode, GraphNode>();         // Associates nodes with duplicates in insideGraph, so that nodes shared between triangles can be maintained
            truncatedEdgeCatalogue = new Dictionary<GraphEdge, GraphEdge>();           // Associates edges with their clipped version, so each edge is truncated only once even if it is shared by triangles

            // Iterate through every triangle in Graph seeing if it has been clipped
            List<GraphTriangle> triangles = outside.Triangles.ToList();        // Copy collection so can alter original whilst iterating
            foreach (GraphTriangle triangle in triangles)
                TruncateTriangle(triangle, clipEdgePoint1, clipEdgePoint2, insideSide);

            // TODO: Remove all nodes from oldNewNodesDict.Keys() from current graph -> maybe dict doesn't contain all necessary nodes
            foreach (GraphNode node in interGraphDuplicateNodes.Keys)
                outside.Destroy(node);
        }

        private void TruncateTriangle(GraphTriangle triangle, Vector3 clipEdgePoint1, Vector3 clipEdgePoint2, float insideSide)
        {
            // Get nodes, inside, outside, and on the clip edge
            GraphNode[] insideNodes = triangle.Nodes.Where(n => MathExtension.Side(clipEdgePoint1, clipEdgePoint2, n.Vector) == insideSide).ToArray();
            GraphNode[] onEdgeNodes = triangle.Nodes.Where(n => MathExtension.Side(clipEdgePoint1, clipEdgePoint2, n.Vector) == 0).ToArray();
            GraphNode[] outsideNodes = triangle.Nodes.Where(n => MathExtension.Side(clipEdgePoint1, clipEdgePoint2, n.Vector) == -insideSide).ToArray();

            // No intersection - aka Triangle is INSIDE the clip edge - Cut / Paste triangle to insideGraph:
            if (outsideNodes.Length == 0) CreateTriangleInInsideGraph(triangle.Nodes[0], triangle.Nodes[1], triangle.Nodes[2]);

            // Also no intersection
            else if (insideNodes.Length == 0) { /* Nothing */ }

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
                    outside.DefineTriangle(intersectionNodes[0], intersectionNodes[1], outsideNodes.Single());

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
                    outside.DefineTriangle(intersectionNodes[0], intersectionNodes[1], outsideNodes.Single());

                    // Triangulate hole in insideGraph
                    GraphNode[] insideTriangleNodes = TriangulateHoleInInsideGraph(intersectionNodes, insideNodes);

                    // Stitch nodes
                    chunks.Last().AddSplitNode(intersectionNodes[0], insideTriangleNodes[0]);
                    chunks.Last().AddSplitNode(intersectionNodes[1], insideTriangleNodes[1]);
                }
            }
        }

        /// <summary>
        /// Truncates edges of the triangle intersected by the given clipEdge. The clippedGraph is updated to reflect the truncation. The two points of intersection between the clip edge and
        /// triangle are created and returned. An edge is also created between the points of intersection. Where a triangle node falls on the clipEdge no node is created, instead the onEdge
        /// node is returned with the other intersection node.
        /// </summary>
        private GraphNode[] TruncateTriangle(GraphTriangle triangle, GraphNode[] insideNodes, GraphNode[] onEdgeNodes, GraphNode[] outsideNodes, Vector2 clipEdgePoint1, Vector2 clipEdgePoint2)
        {
            // Remember the intersection nodes so they can be triangulated
            GraphNode[] intersectionNodes = new GraphNode[2];
            int index = 0;

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
                        GraphNode intersectionNode = truncatedEdgeCatalogue[clippedEdge].GetOther(insideNode);
                        intersectionNodes[index++] = intersectionNode;
                    }

                    else // EDGE HAS NOT BEEN CLIPPED YET...
                    {
                        // Calculate the intersection of clip edge and tri-edge
                        Vector2 intersection = MathExtension.KnownIntersection(clipEdgePoint1, clipEdgePoint2, insideNode.Vector, outsideNode.Vector);

                        // Create node at intersection, remember node
                        GraphNode intersectionNode = outside.CreateNode(intersection);
                        intersectionNodes[index++] = intersectionNode;

                        // Create an edge from the insideNode to the intersectionNode, add to truncatedEdgeCatalogue
                        GraphEdge insideToIntersectionEdge = outside.CreateEdge(insideNode, intersectionNode);
                        truncatedEdgeCatalogue.Add(clippedEdge, insideToIntersectionEdge);
                    }
                }
            }

            // Create new edge between intersection points
            outside.CreateEdge(intersectionNodes[0], intersectionNodes[1]);

            return intersectionNodes;
        }

        private GraphNode GetOrCreateInsideNode(GraphNode clippedMeshGraphNode)
        {
            if (interGraphDuplicateNodes.ContainsKey(clippedMeshGraphNode))
                return interGraphDuplicateNodes[clippedMeshGraphNode];
            else
            {
                GraphNode node = inside.CreateNode(clippedMeshGraphNode.Vector);
                interGraphDuplicateNodes.Add(clippedMeshGraphNode, node);
                return node;
            }
        }

        private GraphNode[] CreateTriangleInInsideGraph(GraphNode a, GraphNode b, GraphNode c)
        {
            // Get or create nodes
            GraphNode insideA = GetOrCreateInsideNode(a);
            GraphNode insideB = GetOrCreateInsideNode(b);
            GraphNode insideC = GetOrCreateInsideNode(c);

            // Get or create edges
            GraphEdge ab = insideA.HasEdge(insideB) ? insideA.GetEdge(insideB) : inside.CreateEdge(insideA, insideB);
            GraphEdge ac = insideA.HasEdge(insideC) ? insideA.GetEdge(insideC) : inside.CreateEdge(insideA, insideC);
            GraphEdge bc = insideB.HasEdge(insideC) ? insideB.GetEdge(insideC) : inside.CreateEdge(insideB, insideC);

            // Define triangle
            GraphTriangle newTri = inside.DefineTriangle(ab, ac, bc);

            // Return new triangle nodes as array in SAME ORDER as was given to this method
            return new GraphNode[] { insideA, insideB, insideC };
        }

        private void TriangulateHoleInOutsideGraph(IEnumerable<GraphNode> holeNodes)
        {
            // Get node opposite to first node
            GraphNode oppositeNode = null;
            foreach (GraphNode node in holeNodes.Where(n => n != holeNodes.First()))    // Check all nodes except first node
                if (!holeNodes.First().HasEdge(node)) oppositeNode = node;

            // Create edge between first node and opposite node
            GraphEdge intersectingEdge = outside.CreateEdge(holeNodes.First(), oppositeNode);

            // Create triangle between intersecting edge and other nodes in enumeration
            foreach (GraphNode node in holeNodes.Where(n => n != holeNodes.First() && n != oppositeNode))   // Check all nodes except first and opposite
                outside.CreateTriangle(intersectingEdge, node);
        }

        private GraphNode[] TriangulateHoleInInsideGraph(GraphNode[] intersectionNodes, GraphNode[] insideNodes)
        {
            // Add nodes to inside graph and triangulate the hole
            GraphNode a = GetOrCreateInsideNode(intersectionNodes[0]);
            GraphNode b = GetOrCreateInsideNode(intersectionNodes[1]);
            GraphNode c = GetOrCreateInsideNode(insideNodes[0]);
            GraphNode d = GetOrCreateInsideNode(insideNodes[1]);

            // Create edges where necessary
            GraphEdge insideAB = inside.CreateEdge(a, b);
            GraphEdge insideAC = a.HasEdge(c) ? a.GetEdge(c) : inside.CreateEdge(a, c);
            GraphEdge insideAD = a.HasEdge(d) ? a.GetEdge(d) : inside.CreateEdge(a, d);
            GraphEdge insideBD = b.HasEdge(d) ? b.GetEdge(d) : inside.CreateEdge(b, d);
            GraphEdge insideCD = c.HasEdge(d) ? c.GetEdge(d) : inside.CreateEdge(c, d);

            // Define triangles
            inside.DefineTriangle(insideAB, insideAD, insideBD);
            inside.DefineTriangle(insideAD, insideAC, insideCD);

            // Return nodes from inside graph in the order they were given to this method
            return new GraphNode[] { a, b, c, d };
        }
    }
}
