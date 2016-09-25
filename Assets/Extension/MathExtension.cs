using System;
using Graph2D;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine
{
    /// <summary>
    /// Static class extending the unity engine math library
    /// </summary>
    public static class MathExtension
    {
        public static float DistanceFromIncircleCentreToEquilateralVertex(Circle incircle)
        {
            // Length of side of equilateral triangle with 'incircle'
            float sideLength = EquilateralSideLength((float)incircle.Radius);

            // a² + b ² =  c²
            float a = Mathf.Pow((sideLength * 0.5f), 2);
            float b = Mathf.Pow((float)incircle.Radius, 2);

            // Distance from incircle centre to any vert in the equilateral triangle containing incircle
            float distance = Mathf.Sqrt(a + b);
            return distance;
        }

        public static float EquilateralSideLength(float incircleRadius)
        {
            // The side length of an eqilateral triangle with the given incircle radius
            return incircleRadius / ((1f / 6f) * Mathf.Sqrt(3));
        }

        public static Bounds CalculateBounds(IEnumerable<Vector2> vectors)
        {
            // Maximal and minimal vectors
            Vector2 greatestX = vectors.First();
            Vector2 leastestX = vectors.First();
            Vector2 greatestY = vectors.First();
            Vector2 leastestY = vectors.First();

            // Find maximal and minimal vectors
            foreach (Vector2 vector in vectors)
            {
                if (vector.x > greatestX.x)
                    greatestX = vector;
                else if (vector.x < leastestX.x)
                    leastestX = vector;

                if (vector.y > greatestY.y)
                    greatestY = vector;
                else if (vector.y < leastestY.y)
                    leastestY = vector;
            }

            // Difference between maximal and minimal vectors is the size of bounds
            float dX = greatestX.x - leastestX.x;
            float dY = greatestY.y - leastestY.y;
            Vector3 size = new Vector3(dX, dY);

            // Minimal vector plus half the total size gives the centre of the bounds
            float centreX = leastestX.x + (dX / 2);
            float centreY = leastestY.y + (dY / 2);
            Vector3 centre = new Vector3(centreX, centreY);

            return new Bounds(centre, size);
        }

        // TODO: Use side of line test for each GraphEdge vs polygonal centre - instead of this method which requires adjacent poitns to be edges

        public static bool ContainsPoint(Vector2[] polyPoints, Vector2 p)
        { 
           int j = polyPoints.Length - 1;
                bool inside = false; 
           for (int i = 0; i<polyPoints.Length; j = i++) { 
              if ( ((polyPoints[i].y <= p.y && p.y<polyPoints[j].y) || (polyPoints[j].y <= p.y && p.y<polyPoints[i].y)) && 
                 (p.x< (polyPoints[j].x - polyPoints[i].x) * (p.y - polyPoints[i].y) / (polyPoints[j].y - polyPoints[i].y) + polyPoints[i].x)) 
                 inside = !inside; 
           } 
           return inside; 
        }


        private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
        }

        // http://stackoverflow.com/questions/2049582/how-to-determine-if-a-point-is-in-a-2d-triangle
        public static bool TriangleContains(Vector2 pt, Vector2 v1, Vector2 v2, Vector2 v3)
        {
            bool b1, b2, b3;

            b1 = Sign(pt, v1, v2) < 0.0f;
            b2 = Sign(pt, v2, v3) < 0.0f;
            b3 = Sign(pt, v3, v1) < 0.0f;

            return ((b1 == b2) && (b2 == b3));
        }

        public static Circle BoundingCircle(Vector2[] vectors)
        {
            // Calculate average of vectors
            Vector2 sum = Vector2.zero;
            foreach (Vector2 vector in vectors)
                sum += vector;

            // Centre of given vectors is their average
            Vector2 centre = sum / vectors.Length;

            // Calculate distance from centre to furthest vector
            float greatestDistance = 0;
            foreach (Vector2 vector in vectors)
            {
                // If distance to vector is larger, it is the new greatest distance
                float distance = Vector2.Distance(vector, centre);
                if (distance > greatestDistance)
                    greatestDistance = distance;
            }

            // Convert to circle
            return new Circle(centre.x, centre.y, greatestDistance);
        }

        public static Vector2 PolygonCentre(IList<Vector2> polygonPoints)
        {
            float signedArea = PolygonSignedArea(polygonPoints);

            // Cx = 1 / 6A 0Σn-1 (xi + x(i + 1)) * (xi * y(i+1) - x(i + 1) * yi)
            // Cy = 1 / 6A 0Σn-1 (yi + y(i + 1)) * (xi * y(i+1) - x(i + 1) * yi)

            float cx = 0;
            float cy = 0;

            for (int i = 0; i < polygonPoints.Count; i++)
            {
                Vector2 currentPoint = polygonPoints[i];
                Vector2 adjacentPoint = polygonPoints[(i + 1) % polygonPoints.Count];

                // (xi * y(i+1) - x(i + 1) * yi)
                float commonFactor = (currentPoint.x * adjacentPoint.y) - (adjacentPoint.x * currentPoint.y);

                cx += (currentPoint.x + adjacentPoint.x) * commonFactor;    // (xi + x(i + 1))
                cy += (currentPoint.y + adjacentPoint.y) * commonFactor;    // (yi + y(i + 1))
            }

            // 1 / 6A
            float oneOverSixA = 1 / (6 * signedArea);

            cx *= oneOverSixA;
            cy += oneOverSixA;

            return new Vector2(cx, cy);
        }

        /// <summary>
        /// Calculates the area of the polygon described by the given points. The given points do NOT need to be in clockwise order but each adjacent point in the list
        /// must form an edge on the polygon
        /// </summary>
        public static float PolygonSignedArea(IList<Vector2> polygonPoints)
        {
            // A =  1 / 2  0Σn-1 (xi * y(i + 1) - x(i + 1) * yi)
            float signedArea = 0;
            for (int i = 0; i < polygonPoints.Count; i++)
            {
                Vector2 currentPoint = polygonPoints[i];
                Vector2 adjacentPoint = polygonPoints[(i + 1) % polygonPoints.Count];

                signedArea += (currentPoint.x * adjacentPoint.y - adjacentPoint.x - currentPoint.y);
            }

            signedArea *= 0.5f;

            return signedArea;
        }

        public static Vector2 KnownIntersection(Vector2 line1Point1, Vector2 line1Point2, Vector2 line2Point1, Vector2 line2Point2)
        {
            Vector2d intersection = new Vector2d();
            if (!Mathd.LineIntersection(line1Point1, line1Point2, line2Point1, line2Point2, ref intersection))
                throw new ArgumentException("Lines do not intersect");

            return new Vector2((float)intersection.x, (float)intersection.y);
        }

        /// <summary>
        /// Determines which side of a line a point lies on. 0 = on line, -1 = one side, +1 = other side
        /// </summary>
        public static float Side(Vector2 edgePoint1, Vector2 edgePoint2, Vector2 point)
        {
            // position = sign((Bx - Ax) * (Y - Ay) - (By - Ay) * (X - Ax))
            float determinant = (edgePoint2.x - edgePoint1.x) * (point.y - edgePoint1.y) - (edgePoint2.y - edgePoint1.y) * (point.x - edgePoint1.x);

            // Mathf.Sign returns 1 when determinant is 0, so need to test for this
            if (determinant == 0) return 0;
            else return Mathf.Sign(determinant);
        }

        public static Vector2 RandomVectorFromTriangularDistribution(Vector2 origin, float maxDistance)
        {
            // Random vertex represent the distance to move from the origin
            Vector2 direction = Random.insideUnitCircle;

            // Random number from triangular distibution that determines the distance to place point away from the origin
            float distance = RandomTriangular(0, maxDistance, 0);

            // Random vector within 'maxDistance' range that tends towards the origin
            return origin + (direction * distance);
        }

        // https://en.wikipedia.org/wiki/Triangular_distribution
        // Calculates a random number from a triangular distribution
        public static float RandomTriangular(float min, float max, float mid)
        {
            // Generate float from uniform distribution
            float unifrom = Random.Range(0.0f, 1.0f);

            // Mid point in the range 0, 1
            float F = (mid - min) / (max - min);

            // If random number from unifrom distribution occurs on lhs of mid point
            if (unifrom <= F)
                return min + Mathf.Sqrt(unifrom * (max - min) * (mid - min));
            else // ...or occurs on rhs of mid point
                return max - Mathf.Sqrt((1 - unifrom) * (max - min) * (max - mid));
        }

        public static Circle Incircle(Vector2 a, Vector2 b, Vector2 c)
        {
            Vector2d abMid = Vector2.Lerp(a, b, 0.5f);
            Vector2d acMid = Vector2.Lerp(a, c, 0.5f);

            Vector2d abToCDirection = c - abMid;
            Vector2d acToBDirection = b - acMid;

            Vector2d centre = Vector2d.zero;

            // Get intersection of lines (abMid, abToCDirection) -> (acMid, acToBDirection)
            if (!Mathd.LineIntersection(abMid, abMid + abToCDirection, acMid, acMid + acToBDirection, ref centre))
                throw new ArgumentException("Lines do not intersect");

            double radius = Vector2d.Distance(centre, abMid);

            Circle incircle = new Circle(centre.x, centre.y, radius);

            return incircle;
        }

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
            if (!Mathd.LineIntersection(midAB, midAB + perpAB, midAC, midAC + perpAC, ref intersection))
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
