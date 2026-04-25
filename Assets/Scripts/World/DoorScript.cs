using MyInterfaces;
using NaughtyAttributes;
using PlayerScripts;
using Unity.Netcode;
using UnityEngine;

namespace World
{
    [RequireComponent(typeof(NetworkObject))]
    public class DoorScript : NetworkBehaviour, IInteractable
    {
        [SerializeField] private Animator animator;
        [SerializeField] private Animator otherAnimator;
        [AnimatorParam("animator")]
        [SerializeField] private string trigger;
        
        public IInteractable PickupOrDropObject(bool pickUp, Vector3 placePosition)
        {
            return null;
        }

        public IInteractable SecondaryInteract(OwnerPlayer interactor = null)
        {
            OpenDoorServerRpc();
            return null;
        }
        
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void OpenDoorServerRpc()
        {
            animator.SetTrigger(trigger);
            if (!otherAnimator) return;
            otherAnimator.SetTrigger(trigger);
        }

        public string GetInteractText()
        {
            return "Open/Close Door";
        }

        public bool IsInteractedWith()
        {
            return false;
        }
    }
}
