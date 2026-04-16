using NaughtyAttributes;
using PlayerScripts;
using Unity.Netcode;
using UnityEngine;

namespace World
{
    public class DoorScript : NetworkBehaviour, IInteractable
    {
        [SerializeField] private Animator animator;
        [SerializeField] private Animator otherAnimator;
        [AnimatorParam("animator")]
        [SerializeField] private string trigger;
        
        public IInteractable PrimaryInteract(OwnerPlayer interactor, bool startedInteraction = true)
        {
            return null;
        }

        public IInteractable SecondaryInteract(OwnerPlayer interactor)
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

        public string GetInteractName()
        {
            return "Door";
        }

        public bool IsInteractedWith()
        {
            return false;
        }
    }
}
