using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets
{
    public class Circle
    {
        public Vector2 Centre { get { return new Vector2((float)centre.x, (float)centre.y); } }
        public double Radius { get; set; }
        private Vector2d centre;

        public Circle(double x, double y, double radius)
        {
            centre = new Vector2d(x, y);
            Radius = radius;
        }

        public bool Contains(Vector2 point)
        {
            // Convert point to double precision
            Vector2d point2d = new Vector2d(point.x, point.y);

            // Calculate distance
            double distance = Vector2d.Distance(centre, point2d);

            // Also check if numbers are similar enough to each other (due to rounding inaccuracies) to be considered the same number
            bool similarEnough = Mathd.Approximately(distance, Radius);

            // If point is inside radius, on, or close enough to radius of circle
            return distance <= Radius || similarEnough;
        }
    }
}
