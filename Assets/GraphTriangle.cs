using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets
{
    public class GraphTriangle
    {
        public GraphNode[] Nodes { get; private set; }
        public GraphEdge[] Edges { get; private set; }

        public Circle Circumcircle
        {
            get
            {
                // Lazy initialisation of circumcircle
                if (circumcircle == null)
                    circumcircle = MathExtension.Circumcircle(Nodes[0].Vector, Nodes[1].Vector, Nodes[2].Vector);

                return circumcircle;
            }
        }
        private Circle circumcircle;

        public GraphTriangle(GraphNode a, GraphNode b, GraphNode c)
        {
            Nodes = new GraphNode[] { a, b, c };
            Edges = new GraphEdge[Nodes.Length];

            // Find edges that connect nodes
            int edgeIndex = 0;
            for (int i = 0; i < Nodes.Length - 1; i++)
            {
                for (int j = i + 1; j < Nodes.Length; j++)
                {
                    foreach (GraphEdge edge in Nodes[i].Edges)
                    {
                        if (edge.Contains(Nodes[j]))
                            Edges[edgeIndex++] = edge;
                    }
                }
            }
        }

        public GraphTriangle(GraphEdge edge1, GraphEdge edge2, GraphEdge edge3)
        {
            Edges = new GraphEdge[] { edge1, edge2, edge3 };
            Nodes = new GraphNode[Edges.Length];
            int i = 0;

            // Add all unique nodes to node list
            foreach (GraphEdge edge in Edges)
            {
                foreach (GraphNode node in edge.Nodes)
                {
                    if (!Nodes.Contains(node))
                        Nodes[i++] = node;
                }
            }
        }

        public bool Contains(GraphNode node)
        {
            return Nodes.Contains(node);
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

        public bool Contains(GraphEdge edge)
        {
            foreach (GraphEdge myEdge in Edges)
            {
                if (myEdge.Equals(edge))
                    return true;
            }

            return false;
        }

        public bool Contains(params GraphEdge[] edges)
        {
            foreach (GraphEdge edge in edges)
            {
                if (!Contains(edge))
                    return false;
            }

            return true;
        }
    }
}
