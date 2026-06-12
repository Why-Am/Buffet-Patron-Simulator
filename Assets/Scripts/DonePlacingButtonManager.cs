using UnityEngine;
using UnityEngine.EventSystems;

public class DonePlacingButtonManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField]
    private FoodPlacementManager foodPlacementManager;
    private bool done;

    void OnEnable()
    {
        done = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (done) return;
        foodPlacementManager.hoveringOverDoneButton = true;
        foodPlacementManager.foodGhost.SetActive(false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (done) return;
        foodPlacementManager.hoveringOverDoneButton = false;
        foodPlacementManager.foodGhost.SetActive(true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        done = true;
        foodPlacementManager.DonePlacingFood();
    }
}
