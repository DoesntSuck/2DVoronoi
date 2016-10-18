//using UnityEngine;
//using System.Collections;
//using Graph2D;

//namespace Assets
//{
//    [RequireComponent(typeof(MeshFilter))]
//    public class MeshCropTest : MonoBehaviour
//    {
//        public Vector2 Nuclei;
//        public Vector2[] CropPolygon;

//        private Graph meshGraph;

//        void Start()
//        {
//            Mesh mesh = GetComponent<MeshFilter>().mesh;

//            meshGraph = new Graph(mesh);
//            for (int i = 0; i < CropPolygon.Length; i++)
//            {
//                Vector2 current = CropPolygon[i];
//                Vector2 next = CropPolygon[(i + 1) % CropPolygon.Length];

//                float side = MathExtension.Side(current, next, Nuclei);
//                meshGraph.Clip(current, next, side);
//            }

//            GetComponent<MeshFilter>().mesh = meshGraph.ToMesh();
//        }

//        void OnDrawGizmos()
//        {
//            GraphDebug.DrawVector(Nuclei, Color.white, 0.01f);

//            // Draw CropPolygon
//            if (CropPolygon != null)
//            {
//                Gizmos.color = Color.red;
//                for (int i = 0; i < CropPolygon.Length; i++)
//                {
//                    Vector2 current = CropPolygon[i];
//                    Vector2 next = CropPolygon[(i + 1) % CropPolygon.Length];

//                    Gizmos.DrawLine(current, next);
//                }
//            }

//            // Draw the mesh object
//            if (meshGraph != null)
//                GraphDebug.DrawGraph(meshGraph);
//        }
//    }
//}