using UnityEngine;
using System.Collections.Generic;
using Graph2D;

[RequireComponent(typeof(MeshFilter))]
public class Breakable : MonoBehaviour
{
    // TODO: DONT NEED TO ORDER VORONOI CELL NODES AND EDGES CLOCKWISE -> just text that another node is on the SAME side as the polygonal centre when doing clipping

    void OnMouseDown()
    {
        // Mouse position to ray
        Ray screenRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Get the position on collider that was hit. THERE IS ONLY ONE COLLIDER IN SCENE SO ONLY THAT COLLIDER CAN GET HIT
        RaycastHit2D hitInfo = Physics2D.GetRayIntersection(screenRay);

        // Break mesh at position
        Break(hitInfo.point, 2);
    }

    // TODO: adjust seedcloud positions by -transform.position 
    public void Break(Vector2 impactPosition, float impactForce)
    {
        // Generate cloud of points centred at impact position
        Vector2[] nuclei = RandomNuclei(impactPosition, impactForce, 10);

        // Each point converts to the nuclei of a Voronoi cell
        List<Graph> cells = VoronoiTessellation.Create(nuclei);

        Mesh mesh = GetComponent<MeshFilter>().mesh;
        foreach (Graph cell in cells)
        {
            // Create graph that mirrors mesh
            Graph meshGraph = new Graph(mesh);

            foreach (GraphEdge clipEdge in cell.Edges)
                meshGraph.Clip(clipEdge);

            // Convert graph to mesh again
            Mesh croppedMesh = meshGraph.ToMesh();
            if (croppedMesh != null)                // If mesh is not completely cropped
            {
                // Create new object and give it the mesh chunk
                GameObject chunk = GameObject.CreatePrimitive(PrimitiveType.Quad);
                chunk.GetComponent<MeshFilter>().mesh = croppedMesh;
            }
        }

        // Remove this object; it is broke
        Destroy(gameObject);
    }

    /// <summary>
    /// // Generate 'count' number of random nuclei inside 'radius' that tend towards the origin 
    /// </summary>
    private Vector2[] RandomNuclei(Vector2 origin, float radius, int count)     // TODO: Minimum 3 chunks, otherwise the break doesn't make much sense (it becomes either a slice [two nuclei], or no break [one nuclei])
    {
        // Generate 'count' number of random nuclei that tend towards the origin
        Vector2[] nuclei = new Vector2[count];
        for (int i = 0; i < count; i++)
            nuclei[i] = MathExtension.RandomVectorFromTriangularDistribution(origin, radius);

        return nuclei;
    }
}
