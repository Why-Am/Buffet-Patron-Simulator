using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class FoodPlacementManager : MonoBehaviour
{
    public TextMeshProUGUI placingText;
    public Camera mainCamera;
    private GameObject food;

    void Start()
    {
        food = Singleton.Instance.foodToPlace;
        placingText.text = $"Placing {food.name}";
    }

    void Update()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            // TODO: add ghost and rotation

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                GameObject spawnedFood = Instantiate(food, hit.point, Quaternion.Euler(0, 0, 0));
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
