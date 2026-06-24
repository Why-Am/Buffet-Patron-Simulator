using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public void BaseSingleInteract()
    {
        SingleInteract();
    }

    public void BaseOnHoverEnter()
    {
        OnHoverEnter();
    }

    public void BaseOnHoverExit()
    {
        OnHoverExit();
    }

    public void BaseHoldInteractStart()
    {
        HoldInteractStart();
    }

    public void BaseHoldInteractEnd()
    {
        HoldInteractEnd();
    }

    public abstract string GetHintText();
    public abstract InteractableType GetInteractableType();

    protected virtual void SingleInteract()
    {
        // Debug.Log($"Interacted with {gameObject.name}");
    }

    protected virtual void HoldInteractStart()
    {

    }

    protected virtual void HoldInteractEnd()
    {

    }

    protected virtual void OnHoverEnter()
    {
        // Debug.Log($"Started hovering over {gameObject.name}");
    }

    protected virtual void OnHoverExit()
    {
        // Debug.Log($"Stopped hovering over {gameObject.name}");
    }
}

public enum InteractableType
{
    SingleInteract,
    SingleInteractAndChangeScene,
    HoldInteract,
}