public class PlateExchange : Interactable
{
    public FirstPersonController firstPersonController;

    public override string GetHintText()
    {
        return "Press E to exchange plates";
    }

    protected override void Interact()
    {
        base.Interact();
        firstPersonController.ResetPlate();
    }
}
