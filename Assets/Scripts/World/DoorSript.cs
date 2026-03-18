using System;
using Unity.Netcode;
using UnityEngine;

public class DoorSript : MonoBehaviour, IInteractable
{
    [SerializeField] private Animator animator;
    [SerializeField] private Animator otherAnimator;
    
    public IInteractable PrimaryInteract(NetworkBehaviourReference interactor, bool beingPickedUp = true)
    {
        return null;
    }

    public IInteractable SecondaryInteract(NetworkBehaviourReference interactor)
    {
        animator.SetTrigger("Interact");
        if (otherAnimator)
        {
            otherAnimator.SetTrigger("Interact");
        }

        return null;
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
