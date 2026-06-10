using UnityEngine;
using System.Collections.Generic;

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

    public float raycastYOffset = 1f;
    public float raycastMaxDistance = 10f;

    private const int IGNORE_RAYCAST_LAYER = 2;

    private const int DEFAULT_LAYER = 0;

    private Mesh originalMesh;
    private Mesh runtimeDeformedMesh;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    private bool isInitialized;

    private struct DisplacedVertexData
    {
        public Vector2 xzPos;
        public Vector3 displacement;
    }

    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (isInitialized) return;
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();

        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            Debug.LogError($"{gameObject.name} needs a MeshFilter with a valid mesh at startup.");
            return;
        }

        // Cache a pristine copy of the mesh right when the game starts.
        // This ensures we always have a clean baseline to deform from.
        originalMesh = Instantiate(meshFilter.sharedMesh);
        isInitialized = true;
    }

    public void SnapToGroundAndDeform()
    {
        Debug.Log($"SnapToGroundAndDeform called on {gameObject.name}");
        if (originalMesh == null) return;

        SnapToGround();
        Deform();
    }

    private void SnapToGround()
    {
        Vector3[] vertices = originalMesh.vertices;
        float smallestDist = float.MaxValue;
        bool atLeastOneRayHit = false;

        gameObject.layer = IGNORE_RAYCAST_LAYER;
        foreach (Vector3 vertex in vertices)
        {
            Vector3 worldPos = transform.TransformPoint(vertex);
            Vector3 rayStart = worldPos + Vector3.up * raycastYOffset;

            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, raycastMaxDistance))
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

    private void Deform()
    {
        Mesh newDeformedMesh = Instantiate(originalMesh);
        Vector3[] vertices = newDeformedMesh.vertices;

        (float minY, float maxY) = GetYBounds(vertices);
        float totalHeight = maxY - minY;
        float bottomCutoff = minY + totalHeight * bottomCutoffFraction;

        List<DisplacedVertexData> validDisplacements = new List<DisplacedVertexData>();
        Vector3[] displacements = new Vector3[vertices.Length];
        bool[] hasDisplacement = new bool[vertices.Length];

        gameObject.layer = IGNORE_RAYCAST_LAYER;

        // Calculate displacements based on bottom vertices
        for (int i = 0; i < vertices.Length; i++)
        {
            if (vertices[i].y > bottomCutoff) continue;

            Vector3 worldPos = transform.TransformPoint(vertices[i]);
            Vector3 rayStart = worldPos + Vector3.up * raycastYOffset;

            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, raycastMaxDistance))
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

        newDeformedMesh.vertices = vertices;
        newDeformedMesh.RecalculateNormals();
        newDeformedMesh.RecalculateBounds();

        // 2. Clean up the previous frame's runtime deformed mesh from memory to avoid leaks
        if (runtimeDeformedMesh != null)
        {
            Destroy(runtimeDeformedMesh);
        }

        // 3. Update active meshes and cache our reference to the new runtime mesh
        runtimeDeformedMesh = newDeformedMesh;
        meshFilter.sharedMesh = runtimeDeformedMesh;

        meshCollider.sharedMesh = runtimeDeformedMesh;
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

    private void OnDestroy()
    {
        // Absolute safety cleanup when the object is destroyed or the scene ends
        if (originalMesh != null) Destroy(originalMesh);
        if (runtimeDeformedMesh != null) Destroy(runtimeDeformedMesh);
    }
}