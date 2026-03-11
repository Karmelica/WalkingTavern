using System;
using Unity.Netcode;
using UnityEngine;

public class DoorSript : MonoBehaviour, IInteractable
{
    [SerializeField] private Animator animator;
    [SerializeField] private Animator otherAnimator;
    
    public void PrimaryInteract(NetworkBehaviourReference interactor, bool beingPickedUp = true)
    {
    }

    public void SecondaryInteract(NetworkBehaviourReference interactor)
    {
        animator.SetTrigger("Interact");
        if (otherAnimator)
        {
            otherAnimator.SetTrigger("Interact");
        }
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
