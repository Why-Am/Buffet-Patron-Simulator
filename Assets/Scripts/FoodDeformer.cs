// My implementation of a mesh deformer based on the AI-generated 
// StructuralSoftBodyDeformer.cs, made to fit my project requirements.
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class FoodDeformer : MonoBehaviour
{
    [Tooltip("How much of the mesh should be deformed from the bottom. 0 = none, 1 = whole mesh")]
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


    [ContextMenu("Snap to ground and deform food at current position")]
    public void SnapToGroundAndDeform()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            Debug.LogError($"{gameObject.name} needs a MeshFilter with a valid mesh.");
            return;
        }

        MeshCollider meshCollider = GetComponent<MeshCollider>();
        if (meshCollider == null)
        {
            Debug.LogError($"{gameObject.name} needs a MeshCollider.");
            return;
        }

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
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            Debug.LogError($"{gameObject.name} needs a MeshFilter with a valid mesh.");
            return;
        }

#if UNITY_EDITOR
        Undo.RecordObject(transform, "Snap to ground");
#endif

        SnapToGround(meshFilter);
        Debug.Log("Snapped to ground.");
    }

    [ContextMenu("Deform at current position")]
    public void Deform()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            Debug.LogError($"{gameObject.name} needs a MeshFilter with a valid mesh.");
            return;
        }

        MeshCollider meshCollider = GetComponent<MeshCollider>();
        if (meshCollider == null)
        {
            Debug.LogError($"{gameObject.name} needs a MeshCollider.");
            return;
        }

#if UNITY_EDITOR
        Undo.RecordObjects(new Object[] { meshFilter, meshCollider }, "Snap to ground and deform");
#endif

        Deform(meshFilter, meshCollider);
        Debug.Log("Deformed.");
    }

    // Snaps object to ground
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
                    Debug.DrawRay(rayStart, Vector3.down * dist, Color.black, 10);
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
        Mesh deformedMesh = Instantiate(originalMesh);
        Vector3[] vertices = deformedMesh.vertices;

        // Ignore self in raycast
        gameObject.layer = IGNORE_RAYCAST_LAYER;

        (float minY, float maxY) = GetYBounds(vertices);

        float totalHeight = maxY - minY;
        float bottomCutoff = minY + totalHeight * bottomCutoffFraction;

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
            }
        }

        // Apply displacement to all vertices
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 referenceDisplacement = hasDisplacement[i] ?
                displacements[i] :
                GetReferenceDisplacement(vertices[i], hasDisplacement, vertices, displacements);


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

    private static Vector3 GetReferenceDisplacement(Vector3 vertex, bool[] hasDisplacement, Vector3[] vertices, Vector3[] displacements)
    {
        Vector3 bestDisplacement = Vector3.zero;
        float closestXZDistance = float.MaxValue;

        for (int j = 0; j < vertices.Length; j++)
        {
            if (hasDisplacement[j])
            {
                float dist = Vector3.Distance(new Vector2(vertex.x, vertex.z), new Vector2(vertices[j].x, vertices[j].z));
                if (dist < closestXZDistance)
                {
                    closestXZDistance = dist;
                    bestDisplacement = displacements[j];
                }
            }
        }

        return bestDisplacement;
    }

}
