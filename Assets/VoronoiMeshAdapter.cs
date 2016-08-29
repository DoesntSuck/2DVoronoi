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
        public static void Shatter(Mesh mesh, Vector2 impact, float force)
        {
            
        }

        public static void Fit(Collider2D collider, VoronoiGraph voronoi)
        {
            // TODO: Maybe use collider to get closest point on bounds

            // Find all nodes outside the meshes triangles
            // For each of the node's edges, if the edge has a node INSIDE the meshes triangles:
            // Find where the edge intersects with the mesh
            // Add node at this point, connect node to edgeNode that is INSIDE the meshes triangles
            // Remove outside node

            // TODO: Special case: both nodes are outside collider, but edge crosses collider


            // TODO: Create collection of all nodes that are added on the edge of polygon -> Add polygon verts -> connect the dots


            // TODO: Before culling voronoi points, get all MESH verts that are contained in voronoi cell
            // TODO: Cut cell along mesh-tri edge (need to check for tri-edges )
            // TODO: If no mesh verts are contained in 

            // Copy collection so can alter original within foreach loop
            GraphNode[] nodes = voronoi.Nodes.ToArray();
            foreach (GraphNode node in nodes)
            {
                // Is the node an outside node?
                if (!collider.OverlapPoint(node.Vector))
                {
                    foreach (GraphEdge edge in node.Edges)
                    {
                        // Get opposite node, check if it is inside the collider
                        GraphNode insideNode = edge.GetOther(node);
                        if (collider.OverlapPoint(insideNode.Vector))
                        {
                            // Find intersection point
                            RaycastHit2D hitInfo;
                            collider.Linecast(node.Vector, insideNode.Vector, out hitInfo);
                            
                            // Add node at intersection point
                            GraphNode intersectionNode = voronoi.AddNode(hitInfo.point);

                            // Add edge from intersection point to insideNode
                            voronoi.AddEdge(intersectionNode, insideNode);

                            // Get collection of cells that use the outside node
                            List<VoronoiCell> affectedCells = voronoi.GetCells(node);
                            foreach (VoronoiCell cell in affectedCells)
                            {
                                // Replace the outside node with the newly inserted one
                                cell.Remove(node);
                                cell.AddNode(intersectionNode);
                            }
                        }
                    }

                    // Remove outside node from voronoi once all its edges have been dealt with
                    voronoi.Remove(node);
                }
            }
        }

        public static void DoAThing(Graph meshGraph, List<VoronoiCell> cells)
        {
            //HashSet<GraphEdge> insideEdges;
            //HashSet<GraphEdge> outsideEdges;

            //GraphUtility.InsideOutsideEdges(out insideEdges, out outsideEdges, meshGraph.Triangles);

            //foreach (VoronoiCell cell in cells)
            //{
            //    foreach (GraphEdge outsideEdge in outsideEdges)
            //    {
            //        // Get outside edge intersection points with this cell
            //        List<Vector2> intersectionPoints = cell.LineSegmentIntersections(outsideEdge.Nodes[0].Vector, outsideEdge.Nodes[1].Vector);

            //        // If there is an intersection
            //        if (intersectionPoints.Count != 0)
            //        {
            //            // Insert points at each intersection of cell wall-outer edge, create edges where appropriate
            //        }
            //    }
            //}

        }


        public static List<Graph> CropMesh(Mesh mesh, List<VoronoiCell> cells)
        {
            // Crop EACH voronoi cell
            // Clip() outputs a list of vectors, but voronoi cell wants nodes and edges
            List<Graph> intersectionPolygons = new List<Graph>();
            foreach (VoronoiCell cell in cells)
            {
                // Order voronoi cell border nodes in a clockwise order so that points on left of voronoi edges are inside cell.
                cell.OrderByClockwise();
                Graph intersectionPolygon = Clip2(mesh, cell);

                // Add intersection polygon to list
                intersectionPolygons.Add(intersectionPolygon);
            }

            return intersectionPolygons;
        }
        
        public static void Clip(Mesh subjectPolygon, VoronoiCell clipPolygon)
        {
            // Create graph from mesh
            Graph outputGraph = new Graph(subjectPolygon);

            foreach (GraphEdge clipPolygonEdge in clipPolygon.Edges)
            {
                List<GraphEdge> subjectPolygonEdges = new List<GraphEdge>(outputGraph.Edges);
                outputGraph.Clear();

                // Adjacent vector pairs s and e form an edge in subjectPolygon
                for (int i = subjectPolygonEdges.Count - 1; i >= 0; i--)
                {
                    Vector2 s = subjectPolygonEdges[i].Nodes[0].Vector;
                    Vector2 e = subjectPolygonEdges[i].Nodes[1].Vector;

                    // If e is inside clip edge
                    if (MathExtension.Side(clipPolygonEdge.Nodes[0].Vector, clipPolygonEdge.Nodes[1].Vector, e) <= 0)
                    {
                        // If s is outside clip edge
                        if (MathExtension.Side(clipPolygonEdge.Nodes[0].Vector, clipPolygonEdge.Nodes[1].Vector, s) > 0)
                            s = MathExtension.KnownIntersection(s, e, clipPolygonEdge.Nodes[0].Vector, clipPolygonEdge.Nodes[1].Vector);
                    }

                    // e IS NOT inside. Check if s is inside clip edge
                    else if (MathExtension.Side(clipPolygonEdge.Nodes[0].Vector, clipPolygonEdge.Nodes[1].Vector, s) <= 0)
                        e = MathExtension.KnownIntersection(s, e, clipPolygonEdge.Nodes[0].Vector, clipPolygonEdge.Nodes[1].Vector);

                    GraphNode startNode = outputGraph.AddNode(s);
                    GraphNode endNode = outputGraph.AddNode(e);

                    outputGraph.AddEdge(startNode, endNode);
                }
            }
        }

        public static Graph Clip2(Mesh subjectPolygon, VoronoiCell clipPolygon)
        {
            // Create graph from mesh
            Graph outputGraph = new Graph(subjectPolygon);

            foreach (GraphEdge clipEdge in clipPolygon.Edges)
            {
                // Check each tri to see if it is clipped
                // Find which nodes are inside and which are outside
                // if not all the same, then the tri is clipped

                // Two cases:
                // 1. One node is outside clip edge:
                // Intersection nodes will form a four sided polygon

                // 2. Two nods are outside clip edge
                // Intersection nodes will for a triangle

                // Create dict to hold refs to the clipped edges and the new edges that are made to replace them
                Dictionary<GraphEdge, GraphEdge> oldNewEdgeDict = new Dictionary<GraphEdge, GraphEdge>();

                foreach (GraphTriangle subjectTriangle in outputGraph.Triangles)
                {
                    // Check each triangle node to see if it is inside or outside the clip edge
                    List<int> insideIndices = new List<int>();
                    for (int i = 0; i < subjectTriangle.Nodes.Length; i++)
                    {
                        // If node is inside, add its index to the list
                        if (MathExtension.Side(clipEdge.Nodes[0].Vector, clipEdge.Nodes[1].Vector, subjectTriangle.Nodes[0].Vector) <= 0)
                            insideIndices.Add(i);
                    }

                    // Case one: no nodes inside the clip edge, delete the triangle
                    if (insideIndices.Count == 0)
                        outputGraph.Remove(subjectTriangle);

                    // Case: one node inside, two nodes outside, intersection nodes and inside node form a triangle
                    else if (insideIndices.Count == 1)
                    {
                        // Remember the intersection nods so they can be triangulated
                        GraphNode[] intersectionNodes = new GraphNode[2];
                        int index = 0;

                        // Get ref to node on inside of clip edge, iterate through outside nodes
                        GraphNode insideNode = subjectTriangle.Nodes[insideIndices[0]];
                        foreach (GraphNode outsideNode in subjectTriangle.Nodes.Where(n => n != insideNode))
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
                                GraphNode intersectionNode = outputGraph.AddNode(intersection);
                                intersectionNodes[index++] = intersectionNode;

                                // Create an edge from the insideNode to the intersectionNode
                                GraphEdge insideToIntersectionEdge = outputGraph.AddEdge(insideNode, intersectionNode);

                                // Add old and new edge to dict
                                oldNewEdgeDict.Add(clippedEdge, insideToIntersectionEdge);
                            }
                        }

                        // Create a triangle between the inside node and the two intersection nodes
                        outputGraph.AddTriangle(insideNode, intersectionNodes[0], intersectionNodes[1]);
                    }

                    // Case: two nodes inside, one node outside, intersection nodes and inside nodes form a four sided polygon
                    else if (insideIndices.Count == 2)
                    {
                        // Remember the intersection nodes so they can be triangulated
                        GraphNode[] intersectionNodes = new GraphNode[2];
                        int index = 0;

                        // Get list of inside nodes by excluding outside not from list
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
                                GraphNode intersectionNode = outputGraph.AddNode(intersection);
                                intersectionNodes[index++] = intersectionNode;

                                // Create an edge from the insideNode to the intersectionNode
                                GraphEdge insideToIntersectionEdge = outputGraph.AddEdge(insideNode, intersectionNode);

                                // Add old and new edge refs to dict
                                oldNewEdgeDict.Add(clippedEdge, insideToIntersectionEdge);
                            }
                        }

                        // Add an edge between intersection nodes, creates a four sided polygon
                        GraphEdge intersectionEdge = outputGraph.AddEdge(intersectionNodes[0], intersectionNodes[1]);

                        // Create an edge that divides the polygon into two triangles
                        GraphEdge bisectingEdge;

                        // We want to create an edge between an intersection node and a non-adjacent inside node
                        if (intersectionNodes[0].HasEdge(insideNodes[0]))               // If there is an edge, the nodes are adjacent   
                            bisectingEdge = outputGraph.AddEdge(intersectionNodes[1], insideNodes[0]);

                        else
                            bisectingEdge = outputGraph.AddEdge(intersectionNodes[0], insideNodes[0]);

                        // Create two triangles from four sided polygon
                        outputGraph.AddTriangle(insideNodes[0], intersectionNodes[0], intersectionNodes[1]);
                        outputGraph.AddTriangle(insideNodes[1], bisectingEdge.Nodes[0], bisectingEdge.Nodes[1]);
                    }
                }

                // Finished with old edges, remove them from the graph, this will also remove associated triangles
                foreach (GraphEdge oldEdge in oldNewEdgeDict.Keys)
                    outputGraph.Remove(oldEdge);
            }

            return outputGraph;
        }
    }
}
