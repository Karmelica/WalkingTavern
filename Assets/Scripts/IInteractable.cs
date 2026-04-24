using PlayerScripts;
using UnityEngine;

public interface IInteractable
{
    public IInteractable PrimaryInteract(OwnerPlayer interactor, bool startedInteraction = true,
        bool disconnection = false);
    public IInteractable SecondaryInteract(OwnerPlayer interactor);
    public string GetInteractText();
    public bool IsInteractedWith();
}