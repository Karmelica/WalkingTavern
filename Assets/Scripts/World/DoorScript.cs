using NaughtyAttributes;
using PlayerScripts;
using Unity.Netcode;
using UnityEngine;

namespace World
{
    public class DoorScript : MonoBehaviour, IInteractable
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
            OpenDoorRpc();
            return null;
        }
        
        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
        private void OpenDoorRpc()
        {
            animator.SetTrigger(trigger);
            if (otherAnimator)
            {
                otherAnimator.SetTrigger(trigger);
            }
            else
            {
                Debug.LogWarning("No other Animator Set", this);
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
}
