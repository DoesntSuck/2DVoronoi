using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graph2D
{
    public class VoronoiCell : Graph
    {
        public Vector2 Nuclei { get; private set; }

        /// <summary>
        /// Checks whether this Voronoi Cell contains atleast 3
        /// points and edges
        /// </summary>
        public bool IsPolygon { get { return Edges.Count > 2 && Nodes.Count > 2; } }

        public VoronoiCell(Vector2 nuclei) : base()
        {
            Nuclei = nuclei;
        }
    }
}

