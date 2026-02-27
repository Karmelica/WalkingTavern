using Unity.Netcode;
using UnityEngine;

namespace World
{
    public class CaravanScript : MonoBehaviour, IInteractable
    {
        [SerializeField] private Transform room;
        private bool _shouldDrive = false;
        private bool _shouldRotate = false;
        
        private void FixedUpdate()
        {
            if(_shouldDrive)
                transform.Translate(transform.forward * (0.5f * Time.fixedDeltaTime));
            if(_shouldRotate)
            {
                transform.Rotate(transform.up, 5f * Time.fixedDeltaTime);
                room.Rotate(transform.up, 5f * Time.fixedDeltaTime);
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            if(other.transform.parent == null)
            {
                other.transform.SetParent(transform);
            }
        }

        private void OnCollisionExit(Collision other)
        {
            if(other.transform.parent == transform)
            {
                other.transform.SetParent(null);
            }
        }

        public void PrimaryInteract(NetworkBehaviourReference interactor, bool pickingUp = true)
        {
            _shouldRotate = !_shouldRotate;
        }

        public void SecondaryInteract(NetworkBehaviourReference interactor)
        {
            _shouldDrive = !_shouldDrive;
        }

        public string GetInteractName()
        {
            return gameObject.name;
        }

        public bool IsInteractedWith()
        {
            return false;
        }
    }
}
