using Unity.Netcode;
using UnityEngine;

public interface IInteractable
{
    public IInteractable PrimaryInteract(NetworkBehaviourReference interactor, bool beingPickedUp = true);
    public IInteractable SecondaryInteract(NetworkBehaviourReference interactor);
    public string GetInteractName();
    public bool IsInteractedWith();
}