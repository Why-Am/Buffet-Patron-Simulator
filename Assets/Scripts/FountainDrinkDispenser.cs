using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Renderer))]
public class FountainDrinkDispenser : Interactable
{
    [SerializeField]
    private Vector3 glassPositionOffset;
    [SerializeField]
    private string drinkName;
    [SerializeField]
    private Color drinkColor;
    [SerializeField]
    private Glass glass;

    void Start()
    {
        if (!TryGetComponent(out Renderer renderer))
        {
            Debug.LogError("Need a Renderer");
        }

        renderer.material.color = drinkColor;
    }

    public override string GetHintText() => $"Hold E to pour {drinkName}";

    public override InteractableType GetInteractableType() => InteractableType.ContinuousInteract;

    protected override void OnHoverEnter()
    {
        base.OnHoverEnter();
        Singleton.Instance.glassInFountainDrinkDispenser = true;
        GameObject glassObj = Singleton.Instance.PutGlass(transform.TransformPoint(glassPositionOffset), Quaternion.identity);
        glass = glassObj.GetComponent<Glass>();
        if (glass == null)
        {
            Debug.LogError($"{glassObj.name} needs a Glass script");
        }
    }

    protected override void OnHoverExit()
    {
        base.OnHoverExit();
        Singleton.Instance.glassInFountainDrinkDispenser = false;
        glass = null;
    }

    protected override void Interact()
    {
        base.Interact();
        glass.Add(drinkColor);
    }

#if UNITY_EDITOR
    [ContextMenu("Initialize")]
    void Initialize()
    {
        Undo.SetCurrentGroupName($"Initialize FountainDrinkDispenser ${gameObject.name}");
        int group = Undo.GetCurrentGroup();

        Undo.RecordObject(gameObject, "Rename");
        gameObject.name = drinkName.Replace(" ", "") + "DrinkDispenser";

        Undo.CollapseUndoOperations(group);
    }
#endif
}
