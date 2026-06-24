using UnityEngine;
using UnityEngine.InputSystem;

public class CameraPivot : MonoBehaviour
{
    [SerializeField]
    private Transform cameraTransform;
    [SerializeField]
    private FoodPlacementManager foodPlacementManager;

    public float zoomSensitivity;

    private float rotationX;
    private float rotationY;

    public float minCameraZ;
    public float maxCameraZ;
    private Vector2 lastCursorPos;

    void Start()
    {
        Vector2 angles = transform.rotation.eulerAngles;
        rotationX = angles.y;
        rotationY = angles.x;
    }

    // Update is called once per frame
    void Update()
    {
        HandleOrbit();
        HandleZoom();
    }

    void HandleOrbit()
    {
        if (Mouse.current.rightButton.isPressed)
        {
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {

                lastCursorPos = Mouse.current.position.ReadValue();
                Cursor.lockState = CursorLockMode.Locked;
                if (foodPlacementManager != null)
                {
                    foodPlacementManager.foodGhost.SetActive(false);
                }
            }

            Vector2 orbitDelta = Mouse.current.delta.ReadValue();
            float sensitivity = Singleton.Instance.mouseSensitivity * 0.01f;
            rotationX += orbitDelta.x * sensitivity;
            rotationY -= orbitDelta.y * sensitivity;
            rotationY = Mathf.Clamp(rotationY, 0, 90);
            transform.rotation = Quaternion.Euler(rotationY, rotationX, 0);
        }
        else if (Mouse.current.rightButton.wasReleasedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Mouse.current.WarpCursorPosition(lastCursorPos);

            if (foodPlacementManager != null)
            {
                foodPlacementManager.foodGhost.SetActive(true);
            }
        }
    }
    void HandleZoom()
    {
        float cameraMoveValue = Mouse.current.scroll.y.ReadValue();
        if (cameraMoveValue == 0) return;

        float cameraZ = cameraTransform.localPosition.z + cameraMoveValue * zoomSensitivity * (-cameraTransform.localPosition.z);
        cameraZ = Mathf.Clamp(cameraZ, minCameraZ, maxCameraZ);
        cameraTransform.localPosition = Vector3.forward * cameraZ;
        // Debug.Log($"Moved camera Z by {cameraMoveValue * zoomSensitivity}");
    }
}
