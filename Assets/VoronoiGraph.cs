using System;
using System.Collections.Generic;
using System.Linq;
using Graph2D;
using UnityEngine;

namespace Assets
{
    public class VoronoiGraph : Graph
    {
        /// <summary>
        /// The voronoi cells
        /// </summary>
        public List<VoronoiCell> Cells { get; private set; }

        public VoronoiGraph()
            : base()
        {
            Cells = new List<VoronoiCell>();
        }

        /// <summary>
        /// Adds a voronoi cell to this graph with the given nuclei
        /// </summary>
        public VoronoiCell AddCell(Vector2 nuclei)
        {
            // Create cell, add to list of cells
            VoronoiCell cell = new VoronoiCell(nuclei);
            Cells.Add(cell);

            return cell;
        }

        public override void Remove(GraphNode node)
        {
            base.Remove(node);

            foreach (VoronoiCell cell in Cells)
            {
                if (cell.Contains(node))
                    cell.Remove(node);
            }
        }

        public List<VoronoiCell> GetCells(GraphNode node)
        {
            // Create and populate a list of all cells that use the given node
            List<VoronoiCell> cells = new List<VoronoiCell>();
            foreach (VoronoiCell cell in Cells)
            {
                if (cell.Contains(node))
                    cells.Add(cell);
            }

            return cells;
        } 
    }
}
