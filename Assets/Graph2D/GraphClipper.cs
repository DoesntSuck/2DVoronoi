using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Graph2D
{
    public static class GraphClipper
    {
        private static List<SplitGraph> chunks;

        /// <summary>
        /// Cuts a hole in the given mesh, according to the shape of the given polygon graph. The nuclei is a point that lies inside
        /// the polygon. Clipping occurs by extending each edge of the polygon so it is infinite in length and truncating mesh triangles
        /// along the edge.
        /// </summary>
        public static Graph Clip(Graph graph, Graph convexPolygon, Vector2 nuclei)
        {
            chunks = new List<SplitGraph>();
            SplitIntoChunks(graph, convexPolygon, nuclei);

            // Remove the last chunk: its the inside chunk
            chunks.RemoveAt(chunks.Count - 1);
            StitchChunksTogether();

            // Return the left over graph parts that have been stitched back together
            return chunks.First().Outside;
        }

        private static void SplitIntoChunks(Graph graph, Graph convexPolygon, Vector2 nuclei)
        {
            // Each edge of the polygon is extended so it is infinite in length and then used to clip the meshes triangles
            foreach (GraphEdge clipEdge in convexPolygon.Edges)
            {
                // Which side of edge is counted as being inside?
                float insideSide = MathExtension.Side(clipEdge.Nodes[0].Vector, clipEdge.Nodes[1].Vector, nuclei);

                // Split graph so stuff outside edge is in one graph, stuff inside the edge is in another, truncate triangles along the edge
                SplitGraph splitGraph = GraphSplitter.Split(graph, clipEdge.Nodes[0].Vector, clipEdge.Nodes[1].Vector, insideSide);
                chunks.Add(splitGraph);
            }
        }

        private static void StitchChunksTogether()
        {
            foreach (SplitGraph splitGraph in chunks.Reverse<SplitGraph>())
                splitGraph.Outside.Stitch(splitGraph.Inside, splitGraph.SplitNodes);
        }
    }
}
