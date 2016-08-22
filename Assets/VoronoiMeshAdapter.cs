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

        public static List<VoronoiCell> CropCells(Graph meshGraph, List<VoronoiCell> cells)
        {
            // Crop EACH voronoi cell
            // Clip() outputs a list of vectors, but voronoi cell wants nodes and edges
            foreach (VoronoiCell cell in cells)
            {
                List<Vector2> intersectionPolygon = Clip(cell, meshGraph);
            }

            return null;
        }

        public static List<Vector2> Clip(VoronoiCell clipPolygon, Graph subjectPolygon)
        {
            // Get vectors from graph
            IEnumerable<Vector2> polygonVectors = subjectPolygon.Nodes.Select(n => n.Vector);
            List<Vector2> outputList = new List<Vector2>(polygonVectors);

            foreach (GraphEdge edge in clipPolygon.Edges)
            {
                // Copy outputList items to new list
                List<Vector2> inputList = outputList.ToList();
                outputList.Clear();

                // Adjacent vector pairs s and e form an edge in subjectPolygon
                Vector2 s = inputList.Last();
                foreach (Vector2 e in inputList)
                {
                    // If e is inside clip edge
                    if (MathExtension.Side(edge.Nodes[0].Vector, edge.Nodes[1].Vector, e) <= 0)
                    {
                        // If s is outside clip edge
                        if (MathExtension.Side(edge.Nodes[0].Vector, edge.Nodes[1].Vector, s) > 0)
                            outputList.Add(MathExtension.KnownIntersection(s, e, edge.Nodes[0].Vector, edge.Nodes[1].Vector));

                        outputList.Add(e);
                    }

                    // If s is inside clip edge
                    else if (MathExtension.Side(edge.Nodes[0].Vector, edge.Nodes[1].Vector, s) <= 0)
                        outputList.Add(MathExtension.KnownIntersection(s, e, edge.Nodes[0].Vector, edge.Nodes[1].Vector));

                    s = e;
                }
            }

            return outputList;
        }
    }
}
