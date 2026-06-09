using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class FoodPlacementManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI placingText;
    [SerializeField]
    private Camera mainCamera;
    [SerializeField]
    private Material ghostMaterial;

    [HideInInspector]
    public GameObject foodGhost;

    private GameObject foodPrefab;
    private GameObject plate;
    private float yRotationDegrees = 0;

    void Start()
    {
        foodPrefab = Singleton.Instance.foodToPlace;
        InitFoodGhost();
        placingText.text = $"Placing {foodPrefab.name}";
        plate = Singleton.Instance.InstantiatePlateAtOriginIfDoesNotExist();
    }

    void InitFoodGhost()
    {
        foodGhost = Instantiate(foodPrefab);
        var meshRenderer = foodGhost.GetComponent<MeshRenderer>();
        meshRenderer.material = ghostMaterial;
    }

    void Update()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            // TODO: add ghost and rotation
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
}
