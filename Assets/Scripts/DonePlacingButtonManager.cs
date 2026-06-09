using UnityEngine;
using UnityEngine.EventSystems;

public class DonePlacingButtonManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField]
    private FoodPlacementManager foodPlacementManager;

    public void OnPointerEnter(PointerEventData eventData)
    {
        foodPlacementManager.hoveringOverDoneButton = true;
        foodPlacementManager.foodGhost.SetActive(false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        foodPlacementManager.hoveringOverDoneButton = false;
        foodPlacementManager.foodGhost.SetActive(true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        foodPlacementManager.DonePlacingFood();
    }
}
