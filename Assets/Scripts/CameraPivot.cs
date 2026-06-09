using UnityEngine;
using UnityEngine.InputSystem;

public class CameraPivot : MonoBehaviour
{
    public Transform cameraTransform;
    public float rotationSensitivity;
    public float zoomSensitivity;

    private float rotationX;
    private float rotationY;

    public float minCameraZ;
    private float maxCameraZ;

    void Start()
    {
        Vector2 angles = transform.rotation.eulerAngles;
        rotationX = angles.y;
        rotationY = angles.x;

        maxCameraZ = cameraTransform.localPosition.z;
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
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            Vector2 orbitDelta = Mouse.current.delta.ReadValue();
            rotationX += orbitDelta.x * rotationSensitivity;
            rotationY -= orbitDelta.y * rotationSensitivity;
            rotationY = Mathf.Clamp(rotationY, 0, 90);
            transform.rotation = Quaternion.Euler(rotationY, rotationX, 0);
        }
        else if (Mouse.current.rightButton.wasReleasedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    void HandleZoom()
    {
        float cameraMoveValue = Mouse.current.scroll.y.ReadValue();
        if (cameraMoveValue == 0) return;

        float cameraZ = cameraTransform.localPosition.z + cameraMoveValue * zoomSensitivity;
        cameraZ = Mathf.Clamp(cameraZ, minCameraZ, maxCameraZ);
        cameraTransform.localPosition = Vector3.forward * cameraZ;
        // Debug.Log($"Moved camera Z by {cameraMoveValue * zoomSensitivity}");
    }
}
