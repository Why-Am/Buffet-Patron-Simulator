using UnityEngine;

public class FoodDisplay : Interactable
{
    public GameObject food;

    public override string GetHintText() => $"Press E to add {food.name}";

    protected override void Interact()
    {
        base.Interact();
        Singleton.Instance.foodToPlace = food;
        Singleton.Instance.ChangeScene("FoodPlacementScene");
    }
}
