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

    public override string GetHintText()
    {
        return "Press E to exchange plates";
    }

    protected override void Interact()
    {
        base.Interact();
        firstPersonController.ResetPlate();
        audioSource.Play();
    }
}
