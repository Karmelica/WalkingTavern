using System;
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
        private readonly NetworkVariable<Color> _cubeColor = new (Color.white);
        private Vector3 _trackPosition;
        private Vector3 _trackForward;
        private NetworkVariable<bool> _isPickedUp = new (false);
        
        private Rigidbody _rigidbody;

        private void Update()
        {
            SetCubePositionServerRpc();
        }
        
        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }
        
        public void PrimaryInteract(Transform interactor)
        {
            if (interactor != null && !_isPickedUp.Value)
            {
                SetTransformsServerRpc(true, interactor.position, interactor.forward);
            }
            else
            {
                SetTransformsServerRpc(false);
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void SetCubePositionServerRpc()
        {
            _rigidbody.useGravity = !_isPickedUp.Value;
            if (!_isPickedUp.Value) return;
            _rigidbody.linearVelocity = ((_trackPosition + _trackForward * 2f) - transform.position) * CubeVel;
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void SetTransformsServerRpc(bool isPickedUp, Vector3 transformPosition = default, Vector3 transformForward = default)
        {
            _isPickedUp.Value = isPickedUp;
            if (!isPickedUp) return;
            _trackPosition = transformPosition;
            _trackForward = transformForward;
        }
    }
}

