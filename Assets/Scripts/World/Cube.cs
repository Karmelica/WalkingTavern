using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace World
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(NetworkTransform))]
    public class Cube : NetworkBehaviour, IInteractable
    {
        private const float CubeVel = 10f;
        private Transform _interactTransform;
        /*private Vector3 _trackPosition;
        private Vector3 _trackForward;*/
        private NetworkVariable<bool> _isPickedUp = new (false);
        
        private Rigidbody _rigidbody;

        private void Update()
        {
            SetCubePositionServerRpc();
        }
        
        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _interactTransform = transform;
        }
        
        public void PrimaryInteract(NetworkBehaviourReference interactor, bool pickingUp = true)
        {
            SetTransformsServerRpc(interactor, pickingUp);
        }

        public bool IsPickedUp()
        {
            return _isPickedUp.Value;
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetCubePositionServerRpc()
        {
            if (!_isPickedUp.Value) return;
            _rigidbody.linearVelocity = ((_interactTransform.position + _interactTransform.forward * 2f) - transform.position) * CubeVel;
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void SetTransformsServerRpc(NetworkBehaviourReference interactor, bool pickingUp = true)
        {
            if (interactor.TryGet(out Player.Player player))
            {
                _isPickedUp.Value = pickingUp;
                _rigidbody.useGravity = !_isPickedUp.Value;
                _interactTransform = player.GetInteractPoint();
            }
            else
            {
                Debug.LogError("Interactor is not a Player.");
            }
        }
    }
}

