using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets
{
    public class GraphEdge
    {
        public GraphNode[] Nodes { get; private set; }
        public HashSet<GraphTriangle> Triangles { get; private set; }
        public IEnumerable<GraphEdge> Edges
        {
            get
            {
                // Do for both nodes
                foreach (GraphNode node in Nodes)
                {
                    // Iterate through each nodes collection of edges
                    foreach (GraphEdge edge in node.Edges)
                        if (edge != this)
                            yield return edge;      // return each edge
                }
            }
        }

        public GraphEdge(GraphNode node1, GraphNode node2)
        {
            Nodes = new GraphNode[] { node1, node2 };
            Triangles = new HashSet<GraphTriangle>();
        }

        public void AddTriangle(GraphTriangle triangle)
        {
            Triangles.Add(triangle);
        }

        public void RemoveTriangle(GraphTriangle triangle)
        {
            Triangles.Remove(triangle);
        }

        public bool SharesNode(GraphEdge other)
        {
            foreach (GraphNode node in Nodes)
            {
                if (other.Contains(node))
                    return true;
            }

            return false;
        }

        public bool SharesNode(GraphTriangle triangle)
        {
            foreach (GraphNode node in Nodes)
            {
                if (triangle.Contains(node))
                    return true;
            }

            return false;
        }

        public bool Contains(GraphNode node)
        {
            foreach (GraphNode myNode in Nodes)
            {
                if (myNode.Equals(node))
                    return true;
            }
            return false;
        }

        public bool Contains(params GraphNode[] nodes)
        {
            foreach (GraphNode node in nodes)
            {
                if (!Contains(node))
                    return false;
            }

            return true;
        }

        public bool Contains(GraphTriangle triangle)
        {
            return Triangles.Contains(triangle);
        }

        public bool ContainsTriangle(GraphNode node1, GraphNode node2, GraphNode node3)
        {
            foreach (GraphTriangle triangle in Triangles)
            {
                if (triangle.Contains(node1, node2, node3))
                    return true;
            }

            return false;
        }

        public bool ContainsTriangle(GraphEdge edge2, GraphEdge edge3)
        {
            foreach (GraphTriangle triangle in Triangles)
            {
                if (triangle.Contains(edge2, edge3))
                    return true;
            }

            return false;
        }

        public bool Equals(GraphEdge other)
        {
            foreach (GraphNode node in other.Nodes)
            {
                if (!Contains(node))
                    return false;
            }

            return true;
        }
    }
}
