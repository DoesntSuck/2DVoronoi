using UnityEngine;
using Graph2D;

namespace Assets
{
    [ExecuteInEditMode]
    public class CircumcircleTest : MonoBehaviour
    {
        Transform[] children;
        Circle circumcircle;

        // Use this for initialization
        void Awake()
        {
            children = new Transform[transform.childCount];
            for (int i = 0; i < children.Length; i++)
                children[i] = transform.GetChild(i);
        }

        public void OnDrawGizmos()
        {
            if (children != null)
            {
                for (int i = 0; i < children.Length - 1; i++)
                {
                    for (int j = i + 1; j < children.Length; j++)
                    {
                        Gizmos.DrawLine(children[i].position, children[j].position);
                    }
                }

                UnityEditor.Handles.color = Color.red;

                if (circumcircle != null)
                    UnityEditor.Handles.DrawWireDisc(circumcircle.Centre, -Vector3.forward, (float)circumcircle.Radius);
            }
        }

        // Update is called once per frame
        void Update()
        {
            circumcircle = MathExtension.Circumcircle(children[0].position, children[1].position, children[2].position);
        }
    }
}