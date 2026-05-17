using Player;
using PlayerScripts;
using UnityEngine;

namespace MyInterfaces
{
    public interface IInteractable
    {
        public IInteractable PickupOrDropObject(bool pickUp, Vector3 placePosition = default);
        public IInteractable SecondaryInteract(OwnerPlayer interactor = null);
        public string GetInteractText();
        public bool IsInteractedWith();
    }
}