using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Graph2D
{
    public static class GraphClipper
    {
        private static List<Graph> pieces;
        private static List<Dictionary<GraphNode, GraphNode>> piecesSplitNodes;

        /// <summary>
        /// Cuts a hole in the given mesh, according to the shape of the given polygon graph. The nuclei is a point that lies inside
        /// the polygon. Clipping occurs by extending each edge of the polygon so it is infinite in length and truncating mesh triangles
        /// along the edge.
        /// </summary>
        public static void Clip(Graph graph, Graph convexPolygon, Vector2 nuclei, out Graph inside, out Graph outside)
        {
            pieces = new List<Graph>();
            pieces.Add(graph);
            piecesSplitNodes = new List<Dictionary<GraphNode, GraphNode>>();

            SplitIntoChunks(graph, convexPolygon, nuclei);
            StitchChunksTogether();

            inside = pieces.Last();
            outside = pieces[pieces.Count - 2];
        }

        private static void SplitIntoChunks(Graph graph, Graph convexPolygon, Vector2 nuclei)
        {
            // Each edge of the polygon is extended so it is infinite in length and then used to clip the meshes triangles
            foreach (GraphEdge clipEdge in convexPolygon.Edges)
            {
                Vector3 edgePoint1 = clipEdge.Nodes[0].Vector;
                Vector3 edgePoint2 = clipEdge.Nodes[1].Vector;

                // Which side of edge is counted as being inside?
                float insideSide = MathExtension.Side(edgePoint1, edgePoint2, nuclei);

                // Split graph so stuff outside edge is in one graph, stuff inside the edge is in another, truncate triangles along the edge
                Graph insideGraph;
                Dictionary<GraphNode, GraphNode> splitNodes = GraphSplitter.Split(pieces.Last(), out insideGraph, edgePoint1, edgePoint2, insideSide);

                pieces.Add(insideGraph);
                piecesSplitNodes.Add(splitNodes);
            }
        }

        private static void StitchChunksTogether()
        {
            for (int i = 0; i < pieces.Count - 2; i++)
                pieces[i + 1].Stitch(pieces[i], piecesSplitNodes[i]);
        }
    }
}
