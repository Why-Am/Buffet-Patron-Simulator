using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public float speed;
    public float gravity = -9.81f;
    public float lowestPossibleY = -50f;
    public Transform cameraTransform;
    public float interactDistance = 1f;
    public LayerMask interactableLayer;
    public Vector3 plateOffset;
    public TextMeshProUGUI interactHintText;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private CharacterController characterController;
    private Vector3 velocity;
    private bool isGrounded;
    private float xRotation = 0f;
    private GameObject plate;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "BuffetScene")
        {
            Initialize();
        }
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Cursor.lockState = CursorLockMode.None;
    }

    void Initialize()
    {
        Debug.Log("Initializing FirstPersonController");
        Cursor.lockState = CursorLockMode.Locked;
        characterController = GetComponent<CharacterController>();
        startPosition = transform.position;
        startRotation = transform.rotation;

        // Restore position if coming back from another scene
        if (Singleton.Instance.TryGetLastPlayerPositionAndRotation(out Vector3 lastPosition, out Quaternion lastRotation))
        {
            SetPositionAndLook(lastPosition, lastRotation);
        }

        Vector3 plateStartPosition = transform.TransformPoint(plateOffset);
        plate = Singleton.Instance.PutPlate(plateStartPosition, Quaternion.identity);
        Singleton.Instance.SetPlateCollisions(false);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMovement();
        UpdateLook();
        MovePlate();
        UpdateInteraction();
        CheckIfFellOffWorld();
    }

    void UpdateMovement()
    {
        isGrounded = characterController.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        Vector3 move = Vector3.zero;

        if (Keyboard.current.aKey.isPressed)
        {
            move -= transform.right;
        }

        if (Keyboard.current.dKey.isPressed)
        {
            move += transform.right;
        }

        if (Keyboard.current.wKey.isPressed)
        {
            move += transform.forward;
        }

        if (Keyboard.current.sKey.isPressed)
        {
            move -= transform.forward;
        }

        move.Normalize();

        characterController.Move(speed * Time.deltaTime * move);

        velocity.y += gravity * Time.deltaTime;

        characterController.Move(velocity * Time.deltaTime);
    }

    // Necessary because DontDestroyOnLoad only works on root GameObjects
    // TODO: make smooth?
    void MovePlate()
    {
        plate.transform.SetPositionAndRotation(transform.TransformPoint(plateOffset), transform.rotation);
    }

    void UpdateLook()
    {
        Vector2 mouseMovement = mouseSensitivity * Time.deltaTime * Mouse.current.delta.ReadValue();
        xRotation -= mouseMovement.y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0, 0);

        transform.Rotate(Vector3.up * mouseMovement.x);
    }

    void UpdateInteraction()
    {
        Ray ray = new(cameraTransform.position, cameraTransform.forward);

        if (!Physics.Raycast(ray, out RaycastHit hitInfo, interactDistance, interactableLayer))
        {
            // Debug.Log("Raycast failed");
            interactHintText.text = "";
            return;
        }


        if (!hitInfo.collider.TryGetComponent<Interactable>(out var interactable))
        {
            interactHintText.text = "";
            return;
        }

        // Debug.Log($"Hovering over interactable {interactable.name}");
        interactHintText.text = interactable.GetHintText();

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            (Vector3 position, Quaternion rotation) = GetPositionAndLook();
            Singleton.Instance.SetLastPlayerPositionAndRotation(position, rotation);
            interactable.BaseInteract();
        }
    }

    void CheckIfFellOffWorld()
    {
        if (transform.position.y > lowestPossibleY) return;

        Debug.Log("Player fell off world, returning.");
        transform.SetPositionAndRotation(startPosition, startRotation);
        xRotation = 0;
    }

    void SetPositionAndLook(Vector3 position, Quaternion rotation)
    {
        // An active CharacterController will not accept position changes
        characterController.enabled = false;

        Vector3 rotationEuler = rotation.eulerAngles;
        transform.SetPositionAndRotation(position, Quaternion.Euler(0, rotationEuler.y, 0));
        xRotation = rotationEuler.x;

        characterController.enabled = true;
    }

    (Vector3 position, Quaternion rotation) GetPositionAndLook()
    {
        Quaternion rotation = Quaternion.Euler(xRotation, transform.rotation.eulerAngles.y, 0);
        return (transform.position, rotation);
    }
}
