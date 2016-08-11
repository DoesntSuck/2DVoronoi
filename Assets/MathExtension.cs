using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets
{
    public static class MathExtension
    {
        public static Circle Circumcircle(Vector2 a, Vector2 b, Vector2 c)
        {
            Vector2d circumcentre = Circumcentre(new Vector2d(a.x, a.y), new Vector2d(b.x, b.y), new Vector2d(c.x, c.y));
            double distanceOA = Vector2d.Distance(circumcentre, new Vector2d(a.x, a.y));

            return new Circle(circumcentre.x, circumcentre.y, distanceOA);
        }

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

            Vector2d intersection = new Vector2d();
            Mathd.LineIntersection(midAB, perpAB * 100, midAC, perpAC * 100, ref intersection);

            return intersection;
        }

        //public static Vector2 Circumcentre(Vector2 a, Vector2 b, Vector2 c)
        //{
        //    Vector3 ab = b - a; // Side ab of triangle
        //    Vector3 ac = c - a; // Side ac of triangle

        //    // Normal vector of plane created by three triangle vectors
        //    Vector3 normal = Vector3.Cross(ab, ac);

        //    Vector3 m1 = Vector3.Lerp(a, b, 0.5f); // Midpoint of line ab
        //    Vector3 m2 = Vector3.Lerp(a, c, 0.5f); // Midpoint of line ac

        //    // Vectors perpendicular to lines ab, and ac
        //    Vector3 d1 = Vector3.Cross(normal, ab);
        //    Vector3 d2 = Vector3.Cross(normal, ac);

        //    Vector3 intersection = LineIntersection(m1, d1, m2, d2);

        //    return intersection;
        //}

        //public static Vector2d LineIntersection(Vector2d linePoint1, Vector2d lineDirection1, Vector2d linePoint2, Vector2d lineDirection2)
        //{
        //    //u = (q − p) × r / (r × s)
        //    Vector2d p = linePoint1;
        //    Vector2d q = linePoint2;

        //    // Arbitrarily long line length
        //    Vector2d r = p + (lineDirection1 * 100);
        //    Vector2d s = q + (lineDirection2 * 100);

        //    // Calculate equation parts:  u = (q − p) × r / (r × s)
        //    Vector2d pq = q - p;
        //    double pqXR = Cross(pq, r);
        //    double rXs = Cross(r, s);

        //    // u = (q − p) × r / (r × s)
        //    double u = pqXR / rXs;


        //    // p + t r = q + u s
        //    return q + (s * u);
        //}

        //// Gives magnitude of equivalent 3d vector cross
        //private static double Cross(Vector2d v, Vector2d w)
        //{
        //    // v × w to be vx wy − vy wx
        //    return v.x * w.y - v.y * w.x;
        //}

        //public static bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
        //{

        //    Vector3 lineVec3 = linePoint2 - linePoint1;
        //    Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
        //    Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

        //    float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

        //    //is coplanar, and not parrallel
        //    if (Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f)
        //    {
        //        float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
        //        intersection = linePoint1 + (lineVec1 * s);
        //        return true;
        //    }
        //    else
        //    {
        //        intersection = Vector3.zero;
        //        return false;
        //    }
        //}
    }
}
