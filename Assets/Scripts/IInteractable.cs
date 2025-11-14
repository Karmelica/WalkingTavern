using Unity.Netcode;
using UnityEngine;

public interface IInteractable
{
    public void PrimaryInteract(NetworkBehaviourReference interactor, bool pickingUp = true);
    public void SecondaryInteract(NetworkBehaviourReference interactor);
    public string GetInteractName();

    public bool IsPickedUp();
}