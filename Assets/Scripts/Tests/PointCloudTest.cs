using UnityEngine;
using System.Collections;
using Graph2D;

[RequireComponent(typeof(Collider))]
public class PointCloudTest : MonoBehaviour
{
    public float Radius;
    public int Count;

    public Vector3 Origin;
    public Vector3[] points;

    void OnMouseDown()
    {
        Ray screenRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        // Raycast against this objects collider
        if (GetComponent<Collider>().Raycast(screenRay, out hitInfo, 100))
        {
            // Set Origin to click location
            Origin = hitInfo.point;

            // Generate 'Count' number of random Vectors that tend towards Origin
            points = new Vector3[Count];
            for (int i = 0; i < Count; i++)
                points[i] = Geometry.RandomVectorFromTriangularDistribution(Origin, Radius);
        }
    }

    void OnDrawGizmos()
    {
        if (points != null)
        {
            // Draw origin in red
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(Origin, 0.05f);
            Gizmos.color = Color.white;

            // Draw other points in white
            for (int i = 0; i < points.Length; i++)
                Gizmos.DrawSphere(points[i], 0.025f);
        }
    }
}
