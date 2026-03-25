using System;
using PlayerScripts;
using Unity.Netcode;
using UnityEngine;

public class DoorSript : MonoBehaviour, IInteractable
{
    [SerializeField] private Animator animator;
    [SerializeField] private Animator otherAnimator;
    
    public IInteractable PrimaryInteract(OwnerPlayer interactor, bool startedInteraction = true)
    {
        return null;
    }

    public IInteractable SecondaryInteract(OwnerPlayer interactor)
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
