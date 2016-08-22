using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Graph2D;
using UnityEngine;

namespace Assets
{
    public class VoronoiCell
    {
        public Vector2 Nuclei { get; private set; }
        public List<GraphNode> Nodes { get; private set; }
        public List<GraphEdge> Edges { get; private set; }

        public VoronoiCell(Vector2 nuclei)
        {
            Nuclei = nuclei;
            Nodes = new List<GraphNode>();
            Edges = new List<GraphEdge>();
        }

        public void AddNode(GraphNode node)
        {
            Nodes.Add(node);

            // Check for edges bordering this cell
            foreach (GraphEdge edge in node.Edges)
            {
                // If cell already knows about this edges' other node, add the edge to cells border edges
                if (Nodes.Contains(edge.GetOther(node)))
                    Edges.Add(edge);
            }
        }

        public void Remove(GraphNode node)
        {
            // Remove node ref
            Nodes.Remove(node);

            // Remove refs to node's edges
            foreach (GraphEdge edge in node.Edges)
                Edges.Remove(edge);
        }

        public bool Contains(GraphNode node)
        {
            return Nodes.Contains(node);
        }

        public bool Contains(GraphEdge edge)
        {
            return Edges.Contains(edge);
        }

        public IEnumerable<GraphNode> GetAdjacentNodeEnumerator(bool clockwise = false)
        {
            HashSet<GraphNode> visitedNodes = new HashSet<GraphNode>();

            // Start with first node in list
            GraphNode walker = Nodes[0];

            if (clockwise)
            {
                visitedNodes.Add(walker);       // Remember we have visited this node
                yield return walker;            // Return current node

                // Get next node in clockwise direction
                walker = GetNextClockwiseNode(walker);
            }

            // While not all of the nodes have been visited
            while (visitedNodes.Count < Nodes.Count)
            {
                visitedNodes.Add(walker);       // Remember we have visited this node
                yield return walker;            // Return current node

                // Find next adjacent node
                foreach (GraphEdge edge in Edges)
                {
                    if (edge.Contains(walker) && !visitedNodes.Contains(edge.GetOther(walker)))
                        walker = edge.GetOther(walker);
                }
            }
        }

        public GraphNode GetNextClockwiseNode(GraphNode node)
        {
            // Get adjacent nodes, convert from list of nodes to list of vectors
            List<Vector2> polygonPoints = GetAdjacentNodeEnumerator().Select(n => n.Vector).ToList();

            // Calculate centre of cell polygon
            Vector2 centre = MathExtension.PolygonCentre(polygonPoints);

            foreach (GraphEdge edge in Edges)
            {
                // Find adjacent edge, check its other node to see if it is clockwise
                if (edge.Contains(node))
                {
                    GraphNode adjacentNode = edge.GetOther(node);

                    // Check which side the node is on
                    float side = MathExtension.Side(node.Vector, centre, adjacentNode.Vector);

                    // If node is clockwise, return it
                    if (side <= 0)
                        return adjacentNode;
                }
            }

            return null;
        }

        // Return points where the segment enters and leaves the polygon.
        public List<GraphEdge> LineEdgeIntersections(Vector2 segmentPoint1, Vector2 segmentPoint2, out List<Vector2> intersectionPoints)
        {
            // Make lists to hold points of intersection
            List<GraphEdge> intersectedEdges = new List<GraphEdge>();
            intersectionPoints = new List<Vector2>();

            // Check for intersection between the segment and each edge in this cell
            foreach (GraphEdge edge in Edges)
            {
                // See where the edge intersects the segment.
                Vector2d intersection = new Vector2d();
                if (Mathd.LineSegmentIntersection(segmentPoint1, segmentPoint2, edge.Nodes[0].Vector, edge.Nodes[1].Vector, ref intersection))
                {
                    // Add edge and the intersection point to lists
                    intersectedEdges.Add(edge);
                    intersectionPoints.Add(new Vector2((float)intersection.x, (float)intersection.y));
                }
            }   

            // Return the intersected edges
            return intersectedEdges;
        }
    }
}
