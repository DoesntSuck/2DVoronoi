using System;
using UnityEngine;

namespace Graph2D
{
    /// <summary>
    /// Defines a circle in 2d space. Uses double precision x, y, and redius values
    /// </summary>
    public class Circle
    {
        /// <summary>
        /// The centre of this circle in 2d space with floating point precision
        /// </summary>
        public Vector2 Centre { get { return new Vector2((float)Centre2d.x, (float)Centre2d.y); } }

        /// <summary>
        /// The radius of this circle with double point precision
        /// </summary>
        public double Radius { get; set; }

        /// <summary>
        /// The centre of this circle in 2d space with double point precision
        /// </summary>
        private Vector2d Centre2d;

        public Circle(double x, double y, double radius)
        {
            Centre2d = new Vector2d(x, y);
            Radius = radius;
        }

        /// <summary>
        /// Checks if the given point is inside, on, or close enough to the bounds of this circle
        /// </summary>
        public bool Contains(Vector2 point)
        {
            // Convert point to double precision
            Vector2d point2d = new Vector2d(point.x, point.y);

            // Calculate distance
            double distance = Vector2d.Distance(Centre2d, point2d);

            // Also check if numbers are similar enough to each other (due to rounding inaccuracies) to be considered the same number
            bool similarEnough = Mathd.Approximately(distance, Radius);

            // If point is inside, on, or close enough to radius of circle
            return distance <= Radius || similarEnough;
        }
    }
}
