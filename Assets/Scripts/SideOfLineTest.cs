using UnityEngine;
using System.Collections;

public class SideOfLineTest : MonoBehaviour
{
    private Vector3 linePoint1;
    private Vector3 linePoint2;
    private Vector3 point;

	// Use this for initialization
	void Start ()
    {
        linePoint1 = transform.GetChild(0).position;
        linePoint2 = transform.GetChild(1).position;
        point = transform.GetChild(2).position;

        print(MathExtension.Side(linePoint1, linePoint2, point));
	}
	
	void OnDrawGizmos()
    {
        Gizmos.DrawLine(linePoint1, linePoint2);

        Gizmos.DrawSphere(point, 0.01f);
    }
}
