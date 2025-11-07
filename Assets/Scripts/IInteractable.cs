using Unity.Netcode;
using UnityEngine;

public interface IInteractable
{
    public void PrimaryInteract(NetworkBehaviourReference interactor, bool pickingUp = true);
    public string GetInteractName();

    public bool IsPickedUp();
}