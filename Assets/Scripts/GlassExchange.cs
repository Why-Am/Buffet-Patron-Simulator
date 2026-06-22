using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GlassExchange : Interactable
{
    public FirstPersonController firstPersonController;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public override string GetHintText() => "Press E to exchange glasses";

    public override InteractableType GetInteractableType() => InteractableType.SingleInteract;

    protected override void Interact()
    {
        base.Interact();
        firstPersonController.ResetGlass();
        audioSource.Play();
    }
}
