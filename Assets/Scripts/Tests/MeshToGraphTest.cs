//using UnityEngine;
//using System.Collections;
//using Graph2D;

//namespace Assets
//{
//    public class MeshToGraphTest : MonoBehaviour
//    {
//        // Use this for initialization
//        void Start()
//        {
//            MeshFilter meshFilter = GetComponent<MeshFilter>();
//            Mesh mesh = meshFilter.mesh;

//            Graph graph = new Graph(mesh);
//            GraphNode newNode = graph.CreateNode(Vector2.one);


//            meshFilter.mesh = graph.ToMesh();
//        }

//        // Update is called once per frame
//        void Update()
//        {

//        }
//    }
//}