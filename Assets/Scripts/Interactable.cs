using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public void BaseInteract()
    {
        Interact();
    }

    public abstract string GetHintText();

    protected virtual void Interact()
    {
        Debug.Log($"Interacted with {gameObject.name}");
    }
}
