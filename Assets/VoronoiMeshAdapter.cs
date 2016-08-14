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

        public static void Fit(Collider2D collider, Graph voronoi)
        {
            // TODO: Maybe use collider to get closest point on bounds

            // Find all nodes outside the meshes triangles
            // For each of the node's edges, if the edge has a node INSIDE the meshes triangles:
            // Find where the edge intersects with the mesh
            // Add node at this point, connect node to edgeNode that is INSIDE the meshes triangles
            // Remove outside node

            // Copy collection so can alter original within foreach loop
            List<GraphNode> nodes = voronoi.Nodes.ToList();
            foreach (GraphNode node in nodes)
            {
                // Is the node an outside node?
                if (!collider.bounds.Contains(node.Vector))
                {
                    foreach (GraphEdge edge in node.Edges)
                    {
                        // Get opposite node, check if it is inside the collider
                        GraphNode insideNode = edge.GetOther(node);
                        if (collider.bounds.Contains(insideNode.Vector))
                        {
                            // Find intersection point
                            RaycastHit2D hitInfo;
                            collider.Linecast(node.Vector, insideNode.Vector, out hitInfo);

                            // Add node at intersection point
                            GraphNode intersectionNode = voronoi.AddNode(hitInfo.point);

                            // Add edge from intersection point to insideNode
                            voronoi.AddEdge(intersectionNode, insideNode);
                        }
                    }

                    voronoi.Remove(node);
                }
            }
        }
    }
}
