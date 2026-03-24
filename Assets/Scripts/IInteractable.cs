using Unity.Netcode;
using UnityEngine;

public interface IInteractable
{
    public IInteractable PrimaryInteract(NetworkBehaviourReference interactor, bool startedInteraction = true);
    public IInteractable SecondaryInteract(NetworkBehaviourReference interactor);
    public string GetInteractName();
    public bool IsInteractedWith();
}