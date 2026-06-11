using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public float speed = 12f;
    public float gravity = -9.81f;
    public float lowestPossibleY = -50f;
    public Transform cameraTransform;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float xRotation = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        controller = GetComponent<CharacterController>();
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMovement();
        UpdateLook();
        CheckIfFellOffWorld();
    }

    void UpdateMovement()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        bool aKeyDown = Keyboard.current.aKey.isPressed;
        bool dKeyDown = Keyboard.current.dKey.isPressed;
        int x = 0;
        if (aKeyDown ^ dKeyDown)
        {
            x = aKeyDown ? -1 : 1;
        }

        bool wKeyDown = Keyboard.current.wKey.isPressed;
        bool sKeyDown = Keyboard.current.sKey.isPressed;
        int z = 0;
        if (wKeyDown ^ sKeyDown)
        {
            z = sKeyDown ? -1 : 1;
        }

        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * speed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    void UpdateLook()
    {
        Vector2 mouseMovement = Mouse.current.delta.ReadValue() * mouseSensitivity * Time.deltaTime;
        xRotation -= mouseMovement.y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0, 0);

        transform.Rotate(Vector3.up * mouseMovement.x);
    }

    void CheckIfFellOffWorld()
    {
        if (transform.position.y > lowestPossibleY) return;

        Debug.Log("Player fell off world, returning.");
        transform.position = startPosition;
        transform.rotation = startRotation;
        xRotation = 0;
    }
}
