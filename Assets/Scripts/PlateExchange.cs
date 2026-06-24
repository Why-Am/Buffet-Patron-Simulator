using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlateExchange : Interactable
{
    public FirstPersonController firstPersonController;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public override string GetHintText() => "Press E to exchange plates";

    public override InteractableType GetInteractableType() => InteractableType.SingleInteract;

    protected override void SingleInteract()
    {
        base.SingleInteract();
        firstPersonController.ResetPlate();
        audioSource.Play();
    }
}
