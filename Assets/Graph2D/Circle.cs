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
        public Vector2 Centre { get; private set; }

        /// <summary>
        /// The radius of this circle with double point precision
        /// </summary>
        public float Radius { get; set; }

        public Circle(Vector2 centre, float radius)
        {
            Centre = centre;
            Radius = radius;
        }

        /// <summary>
        /// Checks if the given point is inside, on, or close enough to the bounds of this circle
        /// </summary>
        public bool Contains(Vector2 point)
        {
            // Calculate distance
            float distance = Vector2.Distance(Centre, point);

            // Also check if numbers are similar enough to each other (due to rounding inaccuracies) to be considered the same number
            bool similarEnough = Mathf.Approximately(distance, Radius);

            // If point is inside, on, or close enough to radius of circle
            return distance <= Radius || similarEnough;
        }
    }
}
