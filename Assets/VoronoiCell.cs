//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Graph2D;
//using UnityEngine;

//namespace Assets
//{
//    public class VoronoiCell
//    {
//        /// <summary>
//        /// The voronoi nuclei: every point in the voronoi cell is closer to this nuclei than any other cells nuclei
//        /// </summary>
//        public Vector2 Nuclei { get; private set; }

//        public Graph Graph { get; private set; }

//        // Lazy initialized polygonal centre
//        public Vector2 Centre
//        {
//            get
//            {
//                if (polygonCentreCalculated == false)
//                {
//                    // Convert from list of nodes to list of vectors
//                    List<Vector2> polygonPoints = Graph.Nodes.Select(n => n.Vector).ToList();

//                    // Calculate centre of cell polygon
//                    polygonCentre = MathExtension.PolygonCentre(polygonPoints);
//                    polygonCentreCalculated = true;
//                }

//                return polygonCentre;
//            }
//        }
//        private Vector2 polygonCentre;          // The centre of the voronoi cell polygon
//        private bool polygonCentreCalculated;   // Whether the polygonCentre vector is currently accurate

//        /// <summary>
//        /// A new voronoi cell with the given nuclei:  every point in the voronoi cell is closer to this nuclei than any other cells nuclei
//        /// </summary>
//        public VoronoiCell(Vector2 nuclei)
//        {
//            Nuclei = nuclei;
//        }

//        public GraphNode AddNode(Vector2 vector)
//        {
//            return Graph.CreateNode(vector);
//        }

//        public GraphEdge AddEdge(GraphNode node1, GraphNode node2)
//        {
//            GraphEdge edge = Graph.CreateEdge(node1, node2);

//            // Check if voronoi cell is complete

//            return Graph.CreateEdge(node1, node2);
//        }

//        public bool IsComplete()
//        {
//            bool twoEdgesPerNode = Graph.Nodes.Where(n => n.Edges.Count == 2).Count() == Graph.Nodes.Count;

//            bool noDuplicateEdges = Graph.Edges.

//            foreach (GraphNode node in Graph.Nodes)
//            {
//                if (node.Edges.Count != 2)
//                    return false;   
//            }

//            return true;
//        }



//        /// <summary>
//        /// Orders the nodes and edges in this cell in a clockwise order, relative to cell centre, starting at the current first node in list
//        /// </summary>
//        public void OrderByClockwise()
//        {
//            // Order nodes clockwise around polygon centre
//            Nodes = GetAdjacentNodeEnumerator(true).ToList();

//            // Order edges
//            List<GraphEdge> orderedEdges = new List<GraphEdge>(Edges.Count);
//            for (int i = 0; i < Nodes.Count; i++)
//            {
//                // Find edge that connects this node to next clockwise node
//                foreach (GraphEdge edge in Edges)
//                {
//                    if (edge.Contains(Nodes[i]) && edge.Contains(Nodes[(i + 1) % Nodes.Count]))
//                    {
//                        // Edge is found, stop looking
//                        orderedEdges.Add(edge);
//                        break;
//                    }
//                }
//            }
//        }

//        private IEnumerable<GraphNode> GetAdjacentNodeEnumerator(bool clockwise = false)
//        {
//            HashSet<GraphNode> visitedNodes = new HashSet<GraphNode>();

//            // Start with first node in list
//            GraphNode walker = Nodes[0];

//            if (clockwise)
//            {
//                visitedNodes.Add(walker);       // Remember we have visited this node
//                yield return walker;            // Return current node

//                // Get next node in clockwise direction
//                walker = GetNextClockwiseNode(walker);
//            }

//            // While not all of the nodes have been visited
//            while (visitedNodes.Count < Nodes.Count)
//            {
//                visitedNodes.Add(walker);       // Remember we have visited this node
//                yield return walker;            // Return current node

//                // Find next adjacent node
//                foreach (GraphEdge edge in Edges)
//                {
//                    if (edge.Contains(walker) && !visitedNodes.Contains(edge.GetOther(walker)))
//                        walker = edge.GetOther(walker);
//                }
//            }
//        }

//        private GraphNode GetNextClockwiseNode(GraphNode node)
//        {
//            foreach (GraphEdge edge in Edges)
//            {
//                // Find adjacent edge, check its other node to see if it is clockwise
//                if (edge.Contains(node))
//                {
//                    GraphNode adjacentNode = edge.GetOther(node);

//                    // Check which side the node is on
//                    float side = MathExtension.Side(node.Vector, Centre, adjacentNode.Vector);

//                    // If node is clockwise, return it
//                    if (side <= 0)
//                        return adjacentNode;
//                }
//            }

//            return null;
//        }
//    }
//}
