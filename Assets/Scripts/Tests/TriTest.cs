using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Graph2D;

public class TriTest : MonoBehaviour
{
    public Vector2 a;
    public Vector2 b;
    public Vector2 c;

    public bool CalculateCircumcircle;

    private Vector3 midAB;
    private Vector3 midAC;
    private Vector3 perpAB;
    private Vector3 perpAC;

    [Header("Gizmos")]
    public float PointRadius = 0.01f;
    public float PerpDistance = 0.5f;
    public Color PointColour = Color.white;
    public Color LineColour = Color.red;
    public Color PerpColour = Color.blue;
    public Color CircumcircleColour = Color.white;

    private Circle circumcircle;

    private void OnDrawGizmos()
    {
        Color gizmoStartColour = Gizmos.color;
        Color handlesStartColour = Handles.color;

        Gizmos.color = PointColour;
        Gizmos.DrawSphere(a, PointRadius);
        Gizmos.DrawSphere(b, PointRadius);
        Gizmos.DrawSphere(c, PointRadius);

        Gizmos.DrawSphere(midAB, PointRadius * 0.5f);
        Gizmos.DrawSphere(midAC, PointRadius * 0.5f);

        Gizmos.color = LineColour;
        Gizmos.DrawLine(a, b);
        Gizmos.DrawLine(c, b);
        Gizmos.DrawLine(a, c);

        Gizmos.color = PerpColour;
        Gizmos.DrawLine(midAB, midAB + (perpAB * PerpDistance));
        Gizmos.DrawLine(midAC, midAC + (perpAC * PerpDistance));

        if (CalculateCircumcircle)
        {
            Handles.color = CircumcircleColour;
            Handles.DrawWireDisc(circumcircle.Centre, Vector3.forward, circumcircle.Radius);
        }

        Gizmos.color = gizmoStartColour;
        Handles.color = handlesStartColour;
    }

    private void OnValidate()
    {
        Vector3 ab = (b - a).normalized; // Side ab of triangle
        Vector3 ac = (c - a).normalized; // Side ac of triangle

        midAB = Vector3.Lerp(a, b, 0.5f);
        midAC = Vector3.Lerp(a, c, 0.5f);

        // Normal vector of plane created by three triangle vectors
        Vector3 normal = Vector3.Cross(ab, ac).normalized;
        //Vector3 normal = Vector3.forward;

        perpAB = Vector3.Cross(normal, ab);
        perpAC = Vector3.Cross(normal, ac);

        if (CalculateCircumcircle)
            circumcircle = Geometry.Circumcircle(a, b, c);
    }
}
