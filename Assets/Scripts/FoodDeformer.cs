// My implementation of a mesh deformer based on the AI-generated 
// StructuralSoftBodyDeformer.cs, made to fit my project requirements.
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class FoodDeformer : MonoBehaviour
{
    [Tooltip("How much of the mesh should be deformed from the bottom. 0 = none, 1 = whole mesh")]
    [Range(0f, 1f)]
    public float bottomCutoffFraction = 0.5f;

    public bool useStructuralIntegrity = true;


    [Tooltip("Greater values -> less integrity. Only applies if \"Use Structural Integrity\" is true")]
    public float structuralDisintegrity = 1f;

    [Tooltip("Padding to prevent z-fighting")]
    public float padding = 0.001f;

    public float raycastYOffset = 0f;

    public float raycastMaxDistance = 10f;

    private int IGNORE_RAYCAST_LAYER = 2;

    private int DEFAULT_LAYER = 0;

    private struct DisplacedVertexData
    {
        public Vector2 xzPos;
        public Vector3 displacement;
    }

    [ContextMenu("Snap to ground and deform food at current position")]
    public void SnapToGroundAndDeform()
    {
        if (!TryGetRequiredComponents(out MeshFilter meshFilter, out MeshCollider meshCollider)) return;

#if UNITY_EDITOR
        Undo.RecordObjects(new Object[] { transform, meshFilter, meshCollider }, "Snap to ground and deform");
#endif

        SnapToGround(meshFilter);
        Deform(meshFilter, meshCollider);
        Debug.Log("Snapped to ground and deformed.");
    }

    [ContextMenu("Snap to ground at current position")]
    public void SnapToGround()
    {
        if (!TryGetRequiredComponents(out MeshFilter meshFilter, out _)) return;

#if UNITY_EDITOR
        Undo.RecordObject(transform, "Snap to ground");
#endif

        SnapToGround(meshFilter);
        Debug.Log("Snapped to ground.");
    }

    [ContextMenu("Deform at current position")]
    public void Deform()
    {
        if (!TryGetRequiredComponents(out MeshFilter meshFilter, out MeshCollider meshCollider)) return;

#if UNITY_EDITOR
        Undo.RecordObjects(new Object[] { meshFilter, meshCollider }, "Snap to ground and deform");
#endif

        Deform(meshFilter, meshCollider);
        Debug.Log("Deformed.");
    }

    private bool TryGetRequiredComponents(out MeshFilter filter, out MeshCollider collider)
    {
        filter = GetComponent<MeshFilter>();
        collider = GetComponent<MeshCollider>();

        if (filter == null || filter.sharedMesh == null)
        {
            Debug.LogError($"{gameObject.name} needs a MeshFilter with a valid mesh.");
            return false;
        }
        if (collider == null)
        {
            Debug.LogError($"{gameObject.name} needs a MeshCollider.");
            return false;
        }
        return true;
    }

    private void SnapToGround(MeshFilter meshFilter)
    {
        Vector3[] vertices = meshFilter.sharedMesh.vertices;

        gameObject.layer = IGNORE_RAYCAST_LAYER;

        float smallestDist = float.MaxValue;

        bool atLeastOneRayHit = false;

        foreach (Vector3 vertex in vertices)
        {
            Vector3 worldPos = transform.TransformPoint(vertex);
            Vector3 rayStart = worldPos + Vector3.up * raycastYOffset;

            if (Physics.Raycast(new Ray(rayStart, Vector3.down), out RaycastHit hit, raycastMaxDistance))
            {
                atLeastOneRayHit = true;
                Vector3 targetWorldPos = hit.point + (hit.normal * padding);

                float dist = worldPos.y - targetWorldPos.y;
                if (dist < smallestDist)
                {
                    smallestDist = dist;
                }
            }
        }

        if (atLeastOneRayHit)
        {
            transform.position += Vector3.down * smallestDist;
        }

        gameObject.layer = DEFAULT_LAYER;
    }

    private void Deform(MeshFilter meshFilter, MeshCollider meshCollider)
    {
        Mesh originalMesh = meshFilter.sharedMesh;

        // Clean up memory if we are continuously deforming an already generated mesh instance
        if (originalMesh.name.EndsWith("_DeformedInstance"))
        {
            // We duplicate the original asset topology before modifying
            originalMesh = Instantiate(originalMesh);
        }

        Mesh deformedMesh = Instantiate(originalMesh);
        deformedMesh.name = originalMesh.name.Replace("_DeformedInstance", "") + "_DeformedInstance";

        Vector3[] vertices = deformedMesh.vertices;

        // Ignore self in raycast
        gameObject.layer = IGNORE_RAYCAST_LAYER;

        (float minY, float maxY) = GetYBounds(vertices);

        float totalHeight = maxY - minY;
        float bottomCutoff = minY + totalHeight * bottomCutoffFraction;

        List<DisplacedVertexData> validDisplacements = new List<DisplacedVertexData>();
        Vector3[] displacements = new Vector3[vertices.Length];
        bool[] hasDisplacement = new bool[vertices.Length];

        // Calculate displacements based on bottom vertices
        for (int i = 0; i < vertices.Length; i++)
        {
            if (vertices[i].y > bottomCutoff) continue;

            Vector3 worldPos = transform.TransformPoint(vertices[i]);
            Vector3 rayStart = worldPos + Vector3.up * raycastYOffset;

            if (Physics.Raycast(new Ray(rayStart, Vector3.down), out RaycastHit hit, raycastMaxDistance))
            {
                Vector3 targetWorldPos = hit.point + (hit.normal * padding);
                Vector3 targetLocalPos = transform.InverseTransformPoint(targetWorldPos);

                displacements[i] = targetLocalPos - vertices[i];
                hasDisplacement[i] = true;

                validDisplacements.Add(new DisplacedVertexData
                {
                    xzPos = new Vector2(vertices[i].x, vertices[i].z),
                    displacement = displacements[i]
                });
            }
        }

        // Apply displacement to all vertices
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 referenceDisplacement = hasDisplacement[i] ?
                displacements[i] :
                GetReferenceDisplacement(vertices[i], validDisplacements);


            if (useStructuralIntegrity)
            {
                float heightFactor = (maxY - vertices[i].y) / totalHeight * structuralDisintegrity;
                heightFactor = Mathf.Clamp01(heightFactor);

                vertices[i] += referenceDisplacement * heightFactor;
            }
            else
            {
                vertices[i] += referenceDisplacement;
            }
        }

        deformedMesh.vertices = vertices;
        deformedMesh.RecalculateNormals();
        deformedMesh.RecalculateBounds();

        // Prevent memory leak of old generated mesh if it was an instance
        if (meshFilter.sharedMesh != null && meshFilter.sharedMesh.name.EndsWith("_DeformedInstance"))
        {
            DestroyImmediate(meshFilter.sharedMesh);
        }

        meshFilter.sharedMesh = deformedMesh;

        meshCollider.sharedMesh = deformedMesh;

        gameObject.layer = DEFAULT_LAYER;
    }

    private static (float minY, float maxY) GetYBounds(Vector3[] vertices)
    {
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        for (int i = 0; i < vertices.Length; i++)
        {
            if (vertices[i].y < minY) minY = vertices[i].y;
            if (vertices[i].y > maxY) maxY = vertices[i].y;
        }

        return (minY, maxY);
    }

    private static Vector3 GetReferenceDisplacement(Vector3 vertex, List<DisplacedVertexData> validDisplacements)
    {
        if (validDisplacements.Count == 0) return Vector3.zero;

        Vector3 bestDisplacement = Vector3.zero;
        float closestXZDistanceSq = float.MaxValue; // Use square magnitude to avoid heavy Mathf.Sqrt calculations
        Vector2 vertexXZ = new Vector2(vertex.x, vertex.z);

        for (int j = 0; j < validDisplacements.Count; j++)
        {
            float distSq = (vertexXZ - validDisplacements[j].xzPos).sqrMagnitude;
            if (distSq < closestXZDistanceSq)
            {
                closestXZDistanceSq = distSq;
                bestDisplacement = validDisplacements[j].displacement;
            }
        }

        return bestDisplacement;
    }

}
