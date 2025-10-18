using Unity.Netcode;
using UnityEngine;

namespace World
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class Cube : NetworkBehaviour, IInteractable
    {
        private Collider _collider;
        private Rigidbody _rigidbody;
        
        public void PrimaryInteract(Transform pos)
        {
            _rigidbody.isKinematic = true;
            transform.position = pos.position + pos.forward * 1.5f;
            transform.rotation = pos.rotation;
        }

        public override void OnNetworkSpawn()
        {
            _collider = GetComponent<Collider>();
            _rigidbody = GetComponent<Rigidbody>();
        }
    }
}
