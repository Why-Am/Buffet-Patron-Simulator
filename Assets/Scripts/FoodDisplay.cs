using TMPro;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FoodDisplay : Interactable
{
    public GameObject food;

    [ContextMenu("Initialize")]
    void Initialize()
    {
        Undo.SetCurrentGroupName($"Initialize FoodDisplay `{gameObject.name}`");
        int group = Undo.GetCurrentGroup();

        Undo.RecordObject(gameObject, "Rename gameObject");
        gameObject.name = food.name.Replace(" ", "") + "FoodDisplay";

        TextMeshPro label = GetComponentInChildren<TextMeshPro>();
        if (label == null)
        {
            Debug.LogError($"{gameObject.name} needs a TextMeshPro");
            return;
        }
        Undo.RecordObject(label, "Set label");
        label.text = food.name;

        GameObject displayFood = Instantiate(food, transform, false);
        Undo.RegisterCreatedObjectUndo(displayFood, "Create display food");

        if (!displayFood.TryGetComponent(out FoodDeformer foodDeformer))
        {
            Debug.LogError($"{displayFood.name} needs a FoodDeformer");
            return;
        }

        displayFood.transform.localPosition = Vector3.up * 3;
        foodDeformer.Initialize();
        foodDeformer.SnapToGround();

        Undo.CollapseUndoOperations(group);
    }

    public override string GetHintText() => $"Press E to add {food.name}";

    protected override void Interact()
    {
        base.Interact();
        Singleton.Instance.foodToPlace = food;
        Singleton.Instance.ChangeScene("FoodPlacementScene");
    }
}
