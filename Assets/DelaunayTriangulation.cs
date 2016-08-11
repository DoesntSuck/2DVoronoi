using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Graph2D;

namespace Assets
{
    public class DelaunayTriangulation
    {
        public static Vector2 SuperTriangleExtents = new Vector2(2.5f, 2.5f);

        public Graph Graph { get; private set; }
        public List<GraphEdge> InsideEdges { get; private set; }
        public List<GraphEdge> OutsideEdges { get; private set; }
        public List<GraphTriangle> NewTriangles { get; private set; }
        public bool Built { get; private set; }

        private GraphNode[] superTriangleNodes;

        public DelaunayTriangulation()
        {
            Graph = new Graph();
            
            // Add super triangle: triangle large enough to encompass all insertion vectors
            superTriangleNodes = new GraphNode[]
            {
                Graph.AddNode(new Vector2(0, SuperTriangleExtents.y)),
                Graph.AddNode(new Vector2(-SuperTriangleExtents.x, -SuperTriangleExtents.y)),
                Graph.AddNode(new Vector2(SuperTriangleExtents.x, -SuperTriangleExtents.y)),
            };

            // Add edges between node pairs
            for (int i = 0; i < superTriangleNodes.Length - 1; i++)
            {
                for (int j = i + 1; j < superTriangleNodes.Length; j++)
                {
                    Graph.AddEdge(superTriangleNodes[i], superTriangleNodes[j]);
                }
            }

            Graph.AddTriangle(superTriangleNodes[0], superTriangleNodes[1], superTriangleNodes[2]);
        }

        public DelaunayTriangulation(Graph graph)
        {
            Graph = graph;
        }

        public void Build()
        {
            //foreach (GraphNode superTriangleNode in superTriangleNodes)
            //    Graph.Remove(superTriangleNode);

            Built = true;
        }

        public void Insert(Vector2 insertionVector)
        {
            //
            // Create list of triangles that have had their Delaunayness violated
            //

            List<GraphTriangle> guiltyTriangles = new List<GraphTriangle>();
            foreach (GraphTriangle triangle in Graph.Triangles)
            {
                if (triangle.Circumcircle.Contains(insertionVector))
                    guiltyTriangles.Add(triangle);
            }

            // Get list of inside and outside edges
            List<GraphEdge> insideEdges;
            List<GraphEdge> outsideEdges;
            InsideOutsideEdges(out insideEdges, out outsideEdges, guiltyTriangles);

            //
            // Remove inside edges leaving a hole in the triangulation
            //

            foreach (GraphEdge insideEdge in insideEdges)
                Graph.Remove(insideEdge);

            //
            // Triangulate the hole
            //

            // TODO: only add edge one time!
            GraphNode newNode = Graph.AddNode(insertionVector);
            foreach (GraphEdge outsideEdge in outsideEdges)
                Graph.CreateTriangle(outsideEdge, newNode);


            //
            // Remove guilty triangle refs from graph
            //

            foreach (GraphTriangle guiltyTriangle in guiltyTriangles)
                Graph.Remove(guiltyTriangle);
        }


        public IEnumerator GetStepEnumerator(Vector2 insertionVector)
        {
            // Insert new node
            GraphNode newNode = Graph.AddNode(insertionVector);

            //
            // Create list of triangles that have had their Delaunayness violated
            //

            List<GraphTriangle> guiltyTriangles = new List<GraphTriangle>();
            foreach (GraphTriangle triangle in Graph.Triangles)
            {
                if (triangle.Circumcircle.Contains(insertionVector))
                    guiltyTriangles.Add(triangle);
            }

            // Get list of inside and outside edges
            List<GraphEdge> insideEdges;
            List<GraphEdge> outsideEdges;
            InsideOutsideEdges(out insideEdges, out outsideEdges, guiltyTriangles);

            // Save to property so can view outside class
            InsideEdges = insideEdges;
            OutsideEdges = outsideEdges;
            yield return null;

            //
            // Remove inside edges leaving a hole in the triangulation
            //

            foreach (GraphEdge insideEdge in insideEdges)
                Graph.Remove(insideEdge);

            //
            // Remove guilty triangle refs from graph
            //

            foreach (GraphTriangle guiltyTriangle in guiltyTriangles)
                Graph.Remove(guiltyTriangle);

            InsideEdges = null;
            yield return null;

            //
            // Triangulate the hole
            //

            NewTriangles = new List<GraphTriangle>();
            foreach (GraphEdge outsideEdge in outsideEdges)
            {
                GraphTriangle newTriangle = Graph.CreateTriangle(outsideEdge, newNode);
                NewTriangles.Add(newTriangle);
            }

            OutsideEdges = null;
            yield return null;
            NewTriangles = null;
        }

        private List<GraphEdge> Edges(List<GraphTriangle> triangles)
        {
            HashSet<GraphEdge> edges = new HashSet<GraphEdge>();
            foreach (GraphTriangle triangle in triangles)
                edges.Union(triangle.Edges);

            return edges.ToList();
        }

        private void InsideOutsideEdges(out List<GraphEdge> insideEdges, out List<GraphEdge> outsideEdges, List<GraphTriangle> triangles)
        {
            List<GraphEdge> edges = Edges(triangles);
            insideEdges = new List<GraphEdge>();
            outsideEdges = new List<GraphEdge>();

            foreach (GraphTriangle triangle in triangles)
            {
                foreach (GraphEdge edge in triangle.Edges)
                {
                    if (outsideEdges.Contains(edge) || insideEdges.Contains(edge))
                    {
                        outsideEdges.Remove(edge);
                        insideEdges.Add(edge);
                    }

                    else
                        outsideEdges.Add(edge);
                }
            }
        }
    }
}
