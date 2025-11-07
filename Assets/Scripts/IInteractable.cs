using Unity.Netcode;
using UnityEngine;

public interface IInteractable
{
    public void PrimaryInteract(NetworkBehaviourReference interactor, bool pickingUp = true);

    public bool IsPickedUp();
}