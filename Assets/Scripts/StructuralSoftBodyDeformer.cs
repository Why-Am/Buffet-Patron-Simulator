// AI generated
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class StructuralSoftBodyDeformer : MonoBehaviour
{
    [Tooltip("The complex mesh underneath that you want to mold onto.")]
    public Collider targetCollider;

    [Tooltip("How far up from the bottom the raycasting 'detection zone' goes. Keep this relatively low to only sample the base shape.")]
    public float baseDetectionZone = 0.5f;

    [Tooltip("Controls how the deformation fades out as it reaches the top of the object. 1 = Linear, 2 = Quadratic (smoother top), 0.5 = Sharp transition.")]
    public float falloffExponent = 0f;

    [Tooltip("Slight padding to prevent Z-fighting visual glitches.")]
    public float surfaceOffset = 0.01f;

    public float raycastOffset = 10f;

    [ContextMenu("Deform and Push Mesh")]
    public void DeformMesh()
    {
        if (targetCollider == null)
        {
            Debug.LogError("Please assign a Target Collider underneath.");
            return;
        }

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            Debug.LogError("This GameObject needs a MeshFilter with a valid mesh.");
            return;
        }

        Undo.RecordObject(meshFilter, "Structural Deform Mesh");

        Mesh originalMesh = meshFilter.sharedMesh;
        Mesh deformedMesh = Instantiate(originalMesh);
        Vector3[] vertices = deformedMesh.vertices;

        // STEP 1: Find the bounding height limits of the mesh (Min Y and Max Y)
        float minY = float.MaxValue;
        float maxY = float.MinValue;
        for (int i = 0; i < vertices.Length; i++)
        {
            if (vertices[i].y < minY) minY = vertices[i].y;
            if (vertices[i].y > maxY) maxY = vertices[i].y;
        }

        float totalHeight = maxY - minY;
        float detectionCutoff = minY + baseDetectionZone;

        // Array to store how much displacement happens at specific X/Z points
        Vector3[] displacements = new Vector3[vertices.Length];
        bool[] hasDisplacement = new bool[vertices.Length];

        // STEP 2: Calculate displacements based ONLY on the bottom vertices
        for (int i = 0; i < vertices.Length; i++)
        {
            if (vertices[i].y <= detectionCutoff)
            {
                Vector3 worldPos = transform.TransformPoint(vertices[i]);
                Vector3 rayStart = worldPos + Vector3.up * raycastOffset;

                if (targetCollider.Raycast(new Ray(rayStart, Vector3.down), out RaycastHit hit, raycastOffset * 2f))
                {
                    Vector3 targetWorldPos = hit.point + (hit.normal * surfaceOffset);
                    Vector3 targetLocalPos = transform.InverseTransformPoint(targetWorldPos);

                    // Calculate the displacement vector (how much this specific column needs to shift)
                    displacements[i] = targetLocalPos - vertices[i];
                    hasDisplacement[i] = true;
                }
            }
        }

        // STEP 3: Apply the displacement to ALL vertices, fading it out toward the top
        for (int i = 0; i < vertices.Length; i++)
        {
            // Find the closest base vertex displacement to use as a reference for this column
            Vector3 bestDelta = Vector3.zero;
            float closestDist = float.MaxValue;

            for (int j = 0; j < vertices.Length; j++)
            {
                if (hasDisplacement[j])
                {
                    // Compare XZ distance to find the base vertex directly "under" the current vertex
                    float dist = Vector2.Distance(new Vector2(vertices[i].x, vertices[i].z), new Vector2(vertices[j].x, vertices[j].z));
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        bestDelta = displacements[j];
                    }
                }
            }

            // Calculate how high this vertex is relative to the whole object (0 = bottom, 1 = top)
            float heightFactor = (vertices[i].y - minY) / totalHeight;
            heightFactor = Mathf.Clamp01(heightFactor);

            // Calculate the structural falloff (1 at bottom, 0 at top)
            float structuralInfluence = Mathf.Pow(1f - heightFactor, falloffExponent);

            // Apply a fraction of the base movement to this upper vertex
            vertices[i] += bestDelta * structuralInfluence;
        }

        deformedMesh.vertices = vertices;
        deformedMesh.RecalculateNormals();
        deformedMesh.RecalculateBounds();

        meshFilter.sharedMesh = deformedMesh;
        Debug.Log("Structural soft-body deformation complete!");
    }
}