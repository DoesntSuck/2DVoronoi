//using UnityEngine;
//using System.Linq;
//using System.Collections.Generic;
//using Graph2D;

//[RequireComponent(typeof(MeshFilter))]
//public class Breakable : MonoBehaviour
//{
//    public int NucleiCount = 10;

//    void OnMouseDown()
//    {
//        // Mouse position to ray
//        Ray screenRay = Camera.main.ScreenPointToRay(Input.mousePosition);

//        // Get the position on collider that was hit. THERE IS ONLY ONE COLLIDER IN SCENE SO ONLY THAT COLLIDER CAN GET HIT
//        RaycastHit2D hitInfo = Physics2D.GetRayIntersection(screenRay);

//        // Break mesh at position
//        Break(hitInfo.point, 2);
//    }

//    // TODO: adjust seedcloud positions by -transform.position 
//    public void Break(Vector2 impactPosition, float impactForce)
//    {
//        // Generate cloud of points centred at impact position
//        Vector2[] nuclei = RandomNuclei(impactPosition, impactForce, NucleiCount);

//        // Each point converts to the nuclei of a Voronoi cell
//        VoronoiTessellation voronoi = new VoronoiTessellation(MathExtension.BoundingCircle(nuclei));
//        voronoi.Insert(nuclei);

//        Mesh mesh = GetComponent<MeshFilter>().mesh;
//        foreach (Graph cell in voronoi.Cells)
//        {
//            Mesh clippedMesh = MeshClipper.Clip(mesh, cell);

//            if (clippedMesh != null)                // If mesh is not completely cropped
//            {
//                // Create new object and give it the mesh chunk
//                GameObject chunk = GameObject.CreatePrimitive(PrimitiveType.Quad);
//                chunk.GetComponent<MeshFilter>().mesh = clippedMesh;
//            }
//        }

//        // Remove this object; it is broke
//        Destroy(gameObject);
//    }

//    /// <summary>
//    /// // Generate 'count' number of random nuclei inside 'radius' that tend towards the origin 
//    /// </summary>
//    private Vector2[] RandomNuclei(Vector2 origin, float radius, int count)     // TODO: Minimum 3 chunks, otherwise the break doesn't make much sense (it becomes either a slice [two nuclei], or no break [one nuclei])
//    {
//        // Generate 'count' number of random nuclei that tend towards the origin
//        Vector2[] nuclei = new Vector2[count];
//        for (int i = 0; i < count; i++)
//            nuclei[i] = MathExtension.RandomVectorFromTriangularDistribution(origin, radius);

//        return nuclei;
//    }
//}
