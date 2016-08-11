using System;
using Graph2D;

namespace UnityEngine
{
    /// <summary>
    /// Static class extending the unity engine math library
    /// </summary>
    public static class MathExtension
    {
        /// <summary>
        /// Calculates the circumcircle of the triangle defined by the given position vectors
        /// </summary>
        public static Circle Circumcircle(Vector2 a, Vector2 b, Vector2 c)
        {
            // Calculate circumcentre (implicit conversion to double precision vectors)
            Vector2d circumcentre = Circumcentre(a, b, c);

            // Distance from circumcentre to position vector a (double precision)
            double distanceOA = Vector2d.Distance(circumcentre, a);

            return new Circle(circumcentre.x, circumcentre.y, distanceOA);
        }

        /// <summary>
        /// Calculates the circumcentre of the triangle defined by the given position vectors
        /// </summary>
        public static Vector2d Circumcentre(Vector2d a, Vector2d b, Vector2d c)
        {
            Vector3d ab = b - a; // Side ab of triangle
            Vector3d ac = c - a; // Side ac of triangle

            // Normal vector of plane created by three triangle vectors
            Vector3d normal = Vector3d.Cross(ab, ac);

            Vector3d midAB = Vector3d.Lerp(a, b, 0.5); // Midpoint of line ab
            Vector3d midAC = Vector3d.Lerp(a, c, 0.5); // Midpoint of line ac

            // Vectors perpendicular to lines ab, and ac
            Vector3d perpAB = Vector3d.Cross(normal, ab);
            Vector3d perpAC = Vector3d.Cross(normal, ac);

            // Intserection of lines perpAB and perpAC defines circumcentre (also perpBC, but only two lines required for calculation)
            Vector2d intersection = new Vector2d();
            if (!Mathd.LineIntersection(midAB, perpAB * 100, midAC, perpAC * 100, ref intersection))
                throw new ArgumentException("No line intersection");        // Something has gone wrong, there is no intersection

            // Return intersection, hopefully no error thrown
            return intersection;
        }

        /// <summary>
        /// Intersection point of two lines.
        /// </summary>
        [Obsolete("My calculations are not as pro as in the Mathd library use Mathd.LineIntersection() instead")]
        public static Vector2d LineIntersection(Vector2d linePoint1, Vector2d lineDirection1, Vector2d linePoint2, Vector2d lineDirection2)
        {
            //u = (q − p) × r / (r × s)
            Vector2d p = linePoint1;
            Vector2d q = linePoint2;

            // Arbitrarily long line length
            Vector2d r = p + (lineDirection1 * 100);
            Vector2d s = q + (lineDirection2 * 100);

            // Calculate equation parts:  u = (q − p) × r / (r × s)
            Vector2d pq = q - p;
            double pqXR = Cross(pq, r);
            double rXs = Cross(r, s);

            // u = (q − p) × r / (r × s)
            double u = pqXR / rXs;


            // p + t r = q + u s
            return q + (s * u);
        }

        // Gives magnitude of equivalent 3d vector cross
        private static double Cross(Vector2d v, Vector2d w)
        {
            // v × w to be vx wy − vy wx
            return v.x * w.y - v.y * w.x;
        }
    }
}
