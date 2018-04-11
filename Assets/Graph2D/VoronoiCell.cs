using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graph2D
{
    public class VoronoiCell : Graph
    {
        /// <summary>
        /// The nuclei of this cell. Every possible point in a
        /// Voronoi cell is closer to its Nuclei than to any
        /// other cell's nuclei.
        /// </summary>
        public Vector2 Nuclei { get; private set; }

        /// <summary>
        /// Checks whether this Voronoi Cell contains atleast 3
        /// points and edges
        /// </summary>
        public bool IsPolygon { get { return Edges.Count > 2 && Nodes.Count > 2; } }

        /// <summary>
        /// A new Voronoi Cell with the given nuclei. The nuclei is
        /// any point that is guaranteed to be inside the polygon.
        /// </summary>
        /// <param name="nuclei"></param>
        public VoronoiCell(Vector2 nuclei) : base()
        {
            Nuclei = nuclei;
        }
    }
}

