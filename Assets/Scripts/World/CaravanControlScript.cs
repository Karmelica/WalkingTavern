using Unity.Netcode;
using UnityEngine;

namespace World
{
    public class CaravanControlScript : MonoBehaviour, IInteractable
    {
        private bool _shouldDrive = false;
        private bool _shouldRotate = false;
        [SerializeField] private GameObject _caravan;
        [SerializeField] private GameObject _room;
        
        private void FixedUpdate()
        {
            _caravan.transform.position = Vector3.Lerp(_caravan.transform.position, transform.position, 0.2f);
            _caravan.transform.rotation = Quaternion.Lerp(_caravan.transform.rotation, transform.rotation, 0.2f);
            
            _room.transform.rotation = Quaternion.Lerp(_room.transform.rotation, transform.rotation, 0.2f);
            
            if(_shouldDrive)
                transform.Translate(transform.forward * (0.5f * Time.fixedDeltaTime), Space.World);
            if(_shouldRotate) {
                transform.Rotate(transform.up, 5f * Time.fixedDeltaTime);
            }
        }

        public IInteractable PrimaryInteract(NetworkBehaviourReference interactor, bool beingPickedUp = true)
        {
            _shouldRotate = !_shouldRotate;
            return null;
        }

        public IInteractable SecondaryInteract(NetworkBehaviourReference interactor)
        {
            _shouldDrive = !_shouldDrive;
            return null;
        }

        public string GetInteractName()
        {
            return "Caravan";
        }

        public bool IsInteractedWith()
        {
            return false;
        }
    }
}
