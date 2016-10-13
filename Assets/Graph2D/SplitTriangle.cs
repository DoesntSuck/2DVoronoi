using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Graph2D
{
    public class SplitTriangle
    {
        public Graph Graph { get; private set; }
        public GraphTriangle[] InsideTriangles { get; private set; }
        public GraphTriangle[] OutsideTriangles { get; private set; }

        /// <summary>
        /// Collection of edges from Outside graph that have been truncated due to the split edge. The 'Key' edge is the original 'pre-truncation'
        /// edge, the 'Value' edge is the new truncated version.
        /// </summary>
        private Dictionary<GraphEdge, GraphEdge> truncatedEdges;

        public SplitTriangle(Graph graph, GraphTriangle triangle)
        {

        }
    }
}
