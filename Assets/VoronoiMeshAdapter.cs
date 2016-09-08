using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Graph2D;
using UnityEngine;


namespace Assets
{
    public static class VoronoiMeshAdapter
    {
        public static List<Graph> CropMesh(Mesh mesh, List<VoronoiCell> cells)
        {
            // Crop EACH voronoi cell
            // Clip() outputs a list of vectors, but voronoi cell wants nodes and edges
            List<Graph> intersectionPolygons = new List<Graph>();
            foreach (VoronoiCell cell in cells)
            {
                // Order voronoi cell border nodes in a clockwise order so that points on left of voronoi edges are inside cell.
                cell.OrderByClockwise();
                Graph intersectionPolygon = Clip(mesh, cell);

                // Add intersection polygon to list
                intersectionPolygons.Add(intersectionPolygon);
            }

            return intersectionPolygons;
        }

        // Check each tri to see if it is clipped
        // Find which nodes are inside and which are outside
        // if not all the same, then the tri is clipped

        // Two cases:
        // 1. One node is outside clip edge:
        // Intersection nodes will form a four sided polygon

        // 2. Two nods are outside clip edge
        // Intersection nodes will for a triangle

        // Create dict to hold refs to the clipped edges and the new edges that are made to replace them

        /// <summary>
        /// Find the indices of the triangle nodes that are 'inside' the given edge
        /// </summary>
        private static List<int> InsideIndices(GraphTriangle triangle, GraphEdge clipEdge)
        {
            List<int> insideIndices = new List<int>();
            for (int i = 0; i < triangle.Nodes.Length; i++)
            {
                // If node is inside, add its index to the list
                if (MathExtension.Side(clipEdge.Nodes[0].Vector, clipEdge.Nodes[1].Vector, triangle.Nodes[0].Vector) <= 0)
                    insideIndices.Add(i);
            }

            return insideIndices;
        }

        private static void CropTriangle(GraphTriangle triangle, GraphEdge clipEdge, Dictionary<GraphEdge, GraphEdge> oldNewEdgeDict)
        {

        }

        public static Graph Clip(Mesh subjectPolygon, VoronoiCell clipPolygon)
        {
            // Create graph from mesh
            Graph outputGraph = new Graph(subjectPolygon);
            List<GraphTriangle> subjectTriangles = outputGraph.Triangles.ToList();

            // Iterate through voronoi edges
            foreach (GraphEdge clipEdge in clipPolygon.Edges)
            {
                // Dict that associates edges with their new, clipped version
                Dictionary<GraphEdge, GraphEdge> oldNewEdgeDict = new Dictionary<GraphEdge, GraphEdge>();

                foreach (GraphTriangle subjectTriangle in subjectTriangles)
                {
                    // Get the indices of all triangle nodes that are 'inside' the given edge
                    List<int> insideIndices = InsideIndices(subjectTriangle, clipEdge);

                    // Case one: no nodes inside the clip edge, delete the triangle
                    if (insideIndices.Count == 0)
                        outputGraph.Remove(subjectTriangle);

                    // Case: one node inside, two nodes outside, intersection nodes and inside node form a triangle
                    else if (insideIndices.Count == 1)
                    {
                        // Remember the intersection nodes so they can be triangulated
                        GraphNode[] intersectionNodes = new GraphNode[2];
                        int intersectionIndex = 0;

                        // Get ref to node on inside of clip edge, get outside node iterator
                        GraphNode insideNode = subjectTriangle.Nodes[insideIndices[0]];
                        IEnumerable<GraphNode> outsideNodes = subjectTriangle.Nodes.Where(n => n != insideNode);

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
                                GraphNode intersectionNode = outputGraph.CreateNode(intersection);
                                intersectionNodes[intersectionIndex++] = intersectionNode;

                                // Create an edge from the insideNode to the intersectionNode
                                GraphEdge insideToIntersectionEdge = outputGraph.CreateEdge(insideNode, intersectionNode);

                                // Add old and new edge to dict
                                oldNewEdgeDict.Add(clippedEdge, insideToIntersectionEdge);
                            }
                        }

                        // Create a triangle between the inside node and the two intersection nodes
                        outputGraph.DefineTriangle(insideNode, intersectionNodes[0], intersectionNodes[1]);
                    }

                    // Case: two nodes inside, one node outside, intersection nodes and inside nodes form a four sided polygon
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
                                GraphNode intersectionNode = outputGraph.CreateNode(intersection);
                                intersectionNodes[index++] = intersectionNode;

                                // Create an edge from the insideNode to the intersectionNode
                                GraphEdge insideToIntersectionEdge = outputGraph.CreateEdge(insideNode, intersectionNode);

                                // Add old and new edge refs to dict
                                oldNewEdgeDict.Add(clippedEdge, insideToIntersectionEdge);
                            }
                        }

                        // Add an edge between intersection nodes, creates a four sided polygon
                        GraphEdge intersectionEdge = outputGraph.CreateEdge(intersectionNodes[0], intersectionNodes[1]);

                        // Create an edge that divides the polygon into two triangles
                        GraphEdge bisectingEdge;

                        // We want to create an edge between an intersection node and a non-adjacent inside node
                        if (intersectionNodes[0].HasEdge(insideNodes[0]))               // If there is an edge, the nodes are adjacent   
                            bisectingEdge = outputGraph.CreateEdge(intersectionNodes[1], insideNodes[0]);

                        else
                            bisectingEdge = outputGraph.CreateEdge(intersectionNodes[0], insideNodes[0]);

                        // Create two triangles from four sided polygon
                        outputGraph.DefineTriangle(insideNodes[0], intersectionNodes[0], intersectionNodes[1]);
                        outputGraph.DefineTriangle(insideNodes[1], bisectingEdge.Nodes[0], bisectingEdge.Nodes[1]);
                    }
                }

                // Finished with old edges, remove them from the graph, this will also remove associated triangles
                foreach (GraphEdge oldEdge in oldNewEdgeDict.Keys)
                    outputGraph.Destroy(oldEdge);
            }

            return outputGraph;
        }
    }
}
