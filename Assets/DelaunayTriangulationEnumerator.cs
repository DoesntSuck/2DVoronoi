using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Graph2D;
using UnityEngine;

namespace Assets
{
    public class DelaunayTriangulationEnumerator : MonoBehaviour
    {
        [Tooltip("The extents of the super triangle that encorporates all nodes in the delaunay triangulation")]
        public Vector2 SuperTriangleExtents = new Vector2(2.5f, 2.5f);

        [Tooltip("The radius in which random vectors are inserted")]
        [Range(0.01f, 5)]
        public float Radius = 1;

        [Tooltip("The minimum distance between each randomly inserted node")]
        [Range(0, 1)]
        public float Spacing = 0.1f;

        [Tooltip("The number of random nodes to insert into the triangulation. If nodes have been specified via the vectors property, no random nodes will be added")]
        public int NodeCount = 10;

        [Tooltip("The number of edges present in the delaunay triangulation")]
        [ReadOnly]
        public int EdgeCount;

        [Tooltip("The number of triangles present in the delaunay triangulation")]
        [ReadOnly]
        public int TriangleCount;

        [Tooltip("The vector value of each node in the delaunay triangulation")]
        public Vector2[] vectors;

        [Tooltip("Should the delaunay triangulation draw the circumcircle of each triangle?")]
        public bool DrawCircumcircle = true;

        public Color NodeColor = Color.white;
        public Color CircumcircleColor = Color.red;
        public Color EdgeColor = Color.yellow;
        public Color TriangleColor = Color.white;
        public Color InsideEdgesColor = Color.black;
        public Color OutsideEdgesColor = Color.green;
        public Color NewTrianglesColor = Color.cyan;
        public Color VoronoiColor = Color.red;

        Graph delaunay;
        VoronoiGraph voronoi;
        GraphNode[] superTriangleNodes;
        HashSet<GraphEdge> insideEdges;
        HashSet<GraphEdge> outsideEdges;
        List<GraphTriangle> newTriangles;
        Coroutine stepwiseDelaunay;

        void Awake()
        {
            // Create graph and insert super triangle
            delaunay = new Graph();


            // Keep debug counts up to date
            EdgeCount = delaunay.Edges.Count;
            TriangleCount = delaunay.Triangles.Count;
        }

        void OnMouseDown()
        {
            if (stepwiseDelaunay == null)
            {
                if (vectors.Length == 0)
                {
                    // Mouse position to ray
                    Ray screenRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                    // Get the position on collider that was hit. THERE IS ONLY ONE COLLIDER IN SCENE SO ONLY THAT COLLIDER CAN GET HIT
                    RaycastHit2D hitInfo = Physics2D.GetRayIntersection(screenRay);

                    // Generate seed cloud of points
                    vectors = GeneratePoints(NodeCount, hitInfo.point, Radius);
                }

                // Start stepping through delaunay triangulation
                stepwiseDelaunay = StartCoroutine(StepwiseDelaunay(vectors));
            }
        }

        private Vector2[] GeneratePoints(int count, Vector2 origin, float distance)
        {
            Vector2[] points = new Vector2[count];

            for (int i = 0; i < count; i++)
            {
                // Generate new vector to be inserted
                Vector2 newVector = MathExtension.RandomVectorFromTriangularDistribution(origin, distance);

                // Get array sub array of vectors that have already been generated
                Vector2[] others = points.SubArray(0, i);

                // Generate a replacement vector until the new vector is not within range of any other vector
                while (newVector.AnyWithinDistance(others, Spacing))
                    newVector = MathExtension.RandomVectorFromTriangularDistribution(origin, distance);

                // Once vector passes test add it to array
                points[i] = newVector;
            }

            return points;
        }

        #region GIZMO DRAWING

        void OnDrawGizmos()
        {
            // Draw delaunay graph
            if (delaunay != null)
            {
                // Draw circumcircles
                if (DrawCircumcircle)   // Draw only circumcircles not relating to the super triangle
                    DrawCircumcircles(delaunay.Triangles.Where(t => !t.ContainsAny(superTriangleNodes)), CircumcircleColor);

                // Draw edges
                DrawEdges(delaunay.Edges, EdgeColor);

                // Draw triangles
                DrawTriangles(delaunay.Triangles, TriangleColor);

                // Draw outside edges
                if (outsideEdges != null)
                    DrawEdges(outsideEdges, OutsideEdgesColor);

                // Draw inside edges
                if (insideEdges != null)
                    DrawEdges(insideEdges, InsideEdgesColor);

                // Draw newly inserted triangles
                if (newTriangles != null)
                    DrawTriangles(newTriangles, NewTrianglesColor);

                // Draw nodes
                DrawNodes(delaunay.Nodes, NodeColor, 0.01f);
            }

            // Draw voronoi graph
            if (voronoi != null)
                DrawEdges(voronoi.Edges, VoronoiColor);
        }

        private void DrawNodes(IEnumerable<GraphNode> nodes, Color color, float radius)
        {
            // Remember original color, set new color
            Color original = Gizmos.color;
            Gizmos.color = color;

            // Draw nodes
            foreach (GraphNode node in nodes)
                Gizmos.DrawSphere(node.Vector, radius);

            // Reset color to original
            Gizmos.color = original;
        }

        private void DrawEdges(IEnumerable<GraphEdge> edges, Color color)
        {
            // Remember original color, set new color
            Color original = Gizmos.color;
            Gizmos.color = color;

            // Draw line between each node
            foreach (GraphEdge edge in edges)
                Gizmos.DrawLine(edge.Nodes[0].Vector, edge.Nodes[1].Vector);

            // Reset color to original
            Gizmos.color = original;
        }

        private void DrawTriangles(IEnumerable<GraphTriangle> triangles, Color color)
        {
            // Remember original color, set new color
            Color original = Gizmos.color;
            Gizmos.color = color;

            // Draw line between each node
            foreach (GraphTriangle triangle in triangles)
            {
                for (int i = 0; i < triangle.Nodes.Length - 1; i++)
                {
                    for (int j = i + 1; j < triangle.Nodes.Length; j++)
                    {
                        Gizmos.DrawLine(triangle.Nodes[i].Vector, triangle.Nodes[j].Vector);
                    }
                }
            }

            // Reset color to original
            Gizmos.color = original;
        }

        private void DrawCircumcircles(IEnumerable<GraphTriangle> triangles, Color color)
        {
            // Remember original color, set new color
            Color original = UnityEditor.Handles.color;
            UnityEditor.Handles.color = color;

            // Draw circumcircle for each triangle
            foreach (GraphTriangle triangle in triangles)
                UnityEditor.Handles.DrawWireDisc(triangle.Circumcircle.Centre, -Vector3.forward, (float)triangle.Circumcircle.Radius);

            // Reset color to original
            UnityEditor.Handles.color = original;
        }

        #endregion

        private IEnumerator WaitForKeyInput(KeyCode keyCode)
        {
            do
            {
                yield return null;
            } while (!Input.GetKeyDown(keyCode));
        }

        private IEnumerator StepwiseDelaunay(Vector2[] vectors)
        {
            // Insert super triangle
            superTriangleNodes = GraphUtility.InsertSuperTriangle(delaunay, Vector2.zero, SuperTriangleExtents.magnitude);

            yield return StartCoroutine(WaitForKeyInput(KeyCode.Space));

            foreach (Vector2 vector in vectors)
            {
                // Insert new node
                GraphNode newNode = delaunay.CreateNode(vector);

                // Create list of triangles that have had their Delaunayness violated
                List<GraphTriangle> guiltyTriangles = GraphUtility.WithinCircumcircles(delaunay.Triangles, vector).ToList();

                // Get list of inside and outside edges
                HashSet<GraphEdge> insideEdges;
                HashSet<GraphEdge> outsideEdges;
                GraphUtility.InsideOutsideEdges(out insideEdges, out outsideEdges, guiltyTriangles);

                // Save to property so can view outside class
                this.insideEdges = insideEdges;
                this.outsideEdges = outsideEdges;
                yield return StartCoroutine(WaitForKeyInput(KeyCode.Space));

                // Remove guilty triangle refs from graph
                foreach (GraphTriangle guiltyTriangle in guiltyTriangles)
                    delaunay.Remove(guiltyTriangle);

                // Remove inside edges leaving a hole in the triangulation
                foreach (GraphEdge insideEdge in insideEdges)
                    delaunay.Destroy(insideEdge);

                this.insideEdges = null;        // Delete ref so inside edges are no longer drawn
                EdgeCount = delaunay.Edges.Count;
                TriangleCount = delaunay.Triangles.Count;
                yield return StartCoroutine(WaitForKeyInput(KeyCode.Space));

                // Triangulate the hole
                newTriangles = new List<GraphTriangle>();
                foreach (GraphEdge outsideEdge in outsideEdges)
                {
                    GraphTriangle newTriangle = delaunay.CreateTriangle(outsideEdge, newNode);
                    newTriangles.Add(newTriangle);
                }

                this.outsideEdges = null;       // Delete ref so outside edges are no longer drawn
                EdgeCount = delaunay.Edges.Count;
                TriangleCount = delaunay.Triangles.Count;
                yield return StartCoroutine(WaitForKeyInput(KeyCode.Space));
                newTriangles = null;            // Delete ref so new triangles are no longer drawn
            }

            yield return StartCoroutine(WaitForKeyInput(KeyCode.Space));

            // Generate voronoi graph
            voronoi = CircumcircleDualGraph();

            yield return StartCoroutine(WaitForKeyInput(KeyCode.Space));

            // Get mesh
            Mesh mesh = GetComponent<MeshFilter>().mesh;

            // Break into chunks based on voronoi cells
            List<Graph> chunks = VoronoiMeshAdapter.CropMesh(mesh, voronoi.Cells);

            GetComponent<MeshFilter>().mesh = GraphUtility.MeshFromGraph(chunks[0]);
        }

        /*
        foreach node in delaunay
            create a new graph (graph is ONE voronoi cell)
            foreach triangle attached to node
                add circumcentre as a border node to graph
            foreach triangle attached to node
                foreach triangle bordering this one
                    create an edge between the two triangles circumcentre nodes
        */

        public List<Graph> VoronoiCells()
        {
            // Voronoi cells
            List<Graph> cells = new List<Graph>();

            foreach (GraphNode node in delaunay.Nodes)
            {
                // Create a new voronoi cell add to list of cells
                Graph cell = new Graph();
                cells.Add(cell);

                // Dictionary to hold association between triangles in delaunay and circumcentre nodes in voronoi cell
                Dictionary<GraphTriangle, GraphNode> triNodeDict = new Dictionary<GraphTriangle, GraphNode>();

                // Create a node in voronoi cell for each triangle attached to the delaunay node
                foreach (GraphTriangle triangle in node.Triangles)
                {
                    GraphNode cellNode = cell.CreateNode(triangle.Circumcircle.Centre);
                    triNodeDict.Add(triangle, cellNode);
                }

                // Create edges between bordering triangles
                foreach (GraphTriangle triangle in node.Triangles)
                {
                    // Get collection of triangles that border this triangle
                    IEnumerable<GraphTriangle> borderingTriangles = node.Triangles.Where(t => t != triangle && t.SharesEdge(triangle));
                    foreach (GraphTriangle borderingTriangle in borderingTriangles)
                    {
                        // Get triangles' associated node in this cell
                        GraphNode node1 = triNodeDict[triangle];
                        GraphNode node2 = triNodeDict[borderingTriangle];
                          
                        // Add an edge between the two nodes
                        cell.CreateEdge(node1, node2);
                    }
                }
            }

            // Return list of voronoi cells
            return cells;
        }

                ///// <summary>
        ///// Creates and returns the voronoi dual graph of this delaunay triangulation. A node is created for each triangle in this graph, the 
        ///// node is position at its associated triangle's circumcentre. Adjacent triangles have their dual nodes connected by an edge.
        ///// </summary>
        //public VoronoiGraph CircumcircleDualGraph()
        //{
        //    // Dict to associate triangles with nodes in dual graph
        //    Dictionary<GraphTriangle, GraphNode> triNodeDict = new Dictionary<GraphTriangle, GraphNode>();
        //    VoronoiGraph dualGraph = new VoronoiGraph();

        //    //
        //    // Add cell border nodes
        //    //

        //    // Create a node for each triangle circumcircle in THIS graph <- constitutes a cell border node
        //    foreach (GraphTriangle triangle in delaunay.Triangles)
        //    {
        //        GraphNode node = dualGraph.AddNode(triangle.Circumcircle.Centre);
        //        triNodeDict.Add(triangle, node);    // Remeber the nodes association to its triangle
        //    }

        //    //
        //    // Add cell border edges
        //    //

        //    // Find triangles that share an edge, create an edge in dual graph connecting their associated nodes
        //    foreach (GraphTriangle triangle1 in delaunay.Triangles)
        //    {
        //        // Compare each triangle to each other triangle
        //        foreach (GraphTriangle triangle2 in delaunay.Triangles.Where(t => t != triangle1))
        //        {
        //            foreach (GraphEdge edge in triangle1.Edges)
        //            {
        //                // Check if triangles share an edge
        //                if (triangle2.Contains(edge))
        //                {
        //                    // Get associated nodes
        //                    GraphNode node1 = triNodeDict[triangle1];
        //                    GraphNode node2 = triNodeDict[triangle2];

        //                    // Add an edge between them
        //                    dualGraph.AddEdge(node1, node2);
        //                }
        //            }
        //        }
        //    }

        //    //
        //    // Add cell nuclei 
        //    //

        //    // Each triangle using this node 
        //    foreach (GraphNode node in delaunay.Nodes)
        //    {
        //        // Add node as a cell nuclei
        //        VoronoiCell cell = dualGraph.AddCell(node.Vector);

        //        // Add each of this nodes triangle's circumcircle nodes to cell
        //        foreach (GraphTriangle triangle in node.Triangles)
        //            cell.AddNode(triNodeDict[triangle]);
        //    }

        //    return dualGraph;
        //}
    }
}
