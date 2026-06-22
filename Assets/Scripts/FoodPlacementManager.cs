using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FoodPlacementManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI placingText;
    [SerializeField]
    private Camera mainCamera;
    [SerializeField]
    private Material ghostMaterial;
    [SerializeField]
    private Button doneButton;
    [SerializeField]
    private EventSystem eventSystem;

    [HideInInspector]
    public GameObject foodGhost;

    private GameObject foodPrefab;
    private GameObject plate;
    private float yRotationDegrees = 0;

    // Managed by DonePlacingButtonManager.cs
    [HideInInspector]
    public bool hoveringOverDoneButton = false;

    private const int IGNORE_RAYCAST_LAYER = 2;

    void Start()
    {
        foodPrefab = Singleton.Instance.foodToPlace;
        placingText.text = $"Placing: {foodPrefab.name}";

        InitFoodGhost();

        plate = Singleton.Instance.PutPlateAtOrigin();
        Singleton.Instance.SetPlateCollisions(true);
        Singleton.Instance.SetGlassActive(false);
    }

    void InitFoodGhost()
    {
        foodGhost = Instantiate(foodPrefab);
        foodGhost.layer = IGNORE_RAYCAST_LAYER;
        var meshRenderer = foodGhost.GetComponent<MeshRenderer>();
        meshRenderer.material = ghostMaterial;
    }

    void Update()
    {
        if (hoveringOverDoneButton) return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            foodGhost.transform.position = hit.point;

            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                yRotationDegrees += 45;
                foodGhost.transform.rotation = Quaternion.Euler(Vector3.up * yRotationDegrees);
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                // GameObject spawnedFood = Instantiate(food, hit.point, Quaternion.identity, plate.transform); // Not used because it scales the child according to the parent
                GameObject spawnedFood = Instantiate(foodPrefab, hit.point, Quaternion.Euler(Vector3.up * yRotationDegrees));
                spawnedFood.transform.SetParent(plate.transform, true);
                if (!spawnedFood.TryGetComponent(out FoodDeformer foodDeformer))
                {
                    Debug.LogError("The food needs a FoodDeformer component");
                    return;
                }

                foodDeformer.Initialize();
                foodDeformer.SnapToGroundAndDeform();
            }
        }
    }

    public void DonePlacingFood()
    {
        Singleton.Instance.ChangeToPreviousScene();
    }
}
