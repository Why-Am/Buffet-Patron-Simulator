using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public void BaseInteract()
    {
        Interact();
    }

    public void BaseOnHoverEnter()
    {
        OnHoverEnter();
    }

    public void BaseOnHoverExit()
    {
        OnHoverExit();
    }

    public abstract string GetHintText();
    public abstract InteractableType GetInteractableType();

    protected virtual void Interact()
    {
        // Debug.Log($"Interacted with {gameObject.name}");
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
    ContinuousInteract,
}