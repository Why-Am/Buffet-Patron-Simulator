using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    public float speed;
    public float gravity = -9.81f;
    public float lowestPossibleY = -50f;
    public Transform cameraTransform;
    public float interactDistance = 1f;
    public LayerMask interactableLayer;
    public Vector3 plateOffset;
    public Vector3 glassOffset;
    public TextMeshProUGUI interactHintText;
    public SettingsPanelManager settingsPanelManager;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private CharacterController characterController;
    private Vector3 velocity;
    private bool isGrounded;
    private float xRotation = 0f;
    private GameObject plate;
    private GameObject glass;
    private Interactable currentInteractable;

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
        // Debug.Log("Initializing FirstPersonController");
        Cursor.lockState = CursorLockMode.Locked;
        characterController = GetComponent<CharacterController>();
        (startPosition, startRotation) = GetPositionAndLook();

        // Restore position if coming back from another scene
        if (Singleton.Instance.TryGetLastPlayerPositionAndRotation(out Vector3 lastPosition, out Quaternion lastRotation))
        {
            SetPositionAndLook(lastPosition, lastRotation);
        }

        Vector3 plateStartPosition = transform.TransformPoint(plateOffset);
        plate = Singleton.Instance.PutPlate(plateStartPosition, Quaternion.identity);
        Singleton.Instance.SetPlateCollisions(false);

        Vector3 glassStartPosition = transform.TransformPoint(glassOffset);
        glass = Singleton.Instance.PutGlass(glassStartPosition, Quaternion.identity);
        Singleton.Instance.SetGlassActive(true);

        interactHintText.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSettingsPanel();
        if (settingsPanelManager.IsOpen) return;
        CheckIfFellOffWorld();
        UpdateMovement();
        UpdateLook();
        MovePlateAndGlass();
        UpdateInteraction();
    }

    void UpdateSettingsPanel()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            settingsPanelManager.ToggleOpen();
        }
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
    void MovePlateAndGlass()
    {
        plate.transform.SetPositionAndRotation(transform.TransformPoint(plateOffset), transform.rotation);

        if (!Singleton.Instance.glassInFountainDrinkDispenser)
        {
            glass.transform.SetPositionAndRotation(transform.TransformPoint(glassOffset), transform.rotation);
        }
    }

    void UpdateLook()
    {
        Vector2 mouseMovement = Singleton.Instance.mouseSensitivity * Time.deltaTime * Mouse.current.delta.ReadValue();
        xRotation -= mouseMovement.y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0, 0);

        transform.Rotate(Vector3.up * mouseMovement.x);
    }

    void UpdateInteraction()
    {
        void ClearLastInteractable()
        {
            if (currentInteractable != null)
            {
                currentInteractable.BaseOnHoverExit();
                currentInteractable = null;
                interactHintText.text = "";
            }
            return;
        }

        Ray ray = new(cameraTransform.position, cameraTransform.forward);

        if (!Physics.Raycast(ray, out RaycastHit hitInfo, interactDistance, interactableLayer))
        {
            ClearLastInteractable();
            return;
        }

        if (!hitInfo.collider.TryGetComponent<Interactable>(out var interactable))
        {
            ClearLastInteractable();
            return;
        }

        if (interactable != currentInteractable)
        {
            if (currentInteractable != null)
            {
                currentInteractable.BaseOnHoverExit();
            }
            currentInteractable = interactable;
            interactable.BaseOnHoverEnter();
            interactHintText.text = interactable.GetHintText();
        }

        switch (interactable.GetInteractableType())
        {
            case InteractableType.SingleInteract:
                if (Keyboard.current.eKey.wasPressedThisFrame)
                {
                    interactable.BaseInteract();
                }
                break;
            case InteractableType.SingleInteractAndChangeScene:
                if (Keyboard.current.eKey.wasPressedThisFrame)
                {
                    (Vector3 position, Quaternion rotation) = GetPositionAndLook();
                    Singleton.Instance.SetLastPlayerPositionAndRotation(position, rotation);
                    interactable.BaseInteract();
                }
                break;
            case InteractableType.ContinuousInteract:
                if (Keyboard.current.eKey.isPressed)
                {
                    interactable.BaseInteract();
                }
                break;
        }
    }

    void CheckIfFellOffWorld()
    {
        if (transform.position.y > lowestPossibleY) return;

        // Debug.Log("Player fell off world, returning.");
        SetPositionAndLook(startPosition, startRotation);
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

    public void ResetPlate()
    {
        Vector3 plateStartPosition = transform.TransformPoint(plateOffset);
        plate = Singleton.Instance.InstantiateNewPlate(plateStartPosition, Quaternion.identity);
        Singleton.Instance.SetPlateCollisions(false);
    }

    public void ResetGlass()
    {
        Vector3 glassStartPosition = transform.TransformPoint(glassOffset); ;
        glass = Singleton.Instance.InstantiateNewGlass(glassStartPosition, Quaternion.identity);
        Singleton.Instance.SetGlassActive(true);
    }
}
