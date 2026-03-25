using PlayerScripts;

public interface IInteractable
{
    public IInteractable PrimaryInteract(OwnerPlayer interactor, bool startedInteraction = true);
    public IInteractable SecondaryInteract(OwnerPlayer interactor);
    public string GetInteractName();
    public bool IsInteractedWith();
}