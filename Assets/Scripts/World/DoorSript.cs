using System;
using Unity.Netcode;
using UnityEngine;

public class DoorSript : MonoBehaviour, IInteractable
{
    [SerializeField] private Animator animator;
    public void PrimaryInteract(NetworkBehaviourReference interactor, bool pickingUp = true)
    {
    }

    public void SecondaryInteract(NetworkBehaviourReference interactor)
    {
        animator.SetTrigger("Interact");
    }

    public string GetInteractName()
    {
        return "Door";
    }

    public bool IsInteractedWith()
    {
        return false;
    }
}
