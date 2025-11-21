using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace World
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(NetworkTransform))]
    [RequireComponent(typeof(NetworkRigidbody))]
    
    public class MoveableObject : NetworkBehaviour, IInteractable
    {
        #region Variables

        private const float CubeVel = 10f;
        private Transform _interactTransform;
        private NetworkVariable<bool> _isPickedUp = new (false);
        
        private Rigidbody _rigidbody;

        #endregion

        #region Unity Methods

        private void Update()
        {
            //SetObjectPositionServerRpc();
            SetObjectPosition();
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _interactTransform = transform;
        }

        #endregion
        
        #region RPC Methods

        /*[Rpc(SendTo.Server)]
        private void SetObjectPositionServerRpc()
        {
            if (!_isPickedUp.Value) return;
            _rigidbody.linearVelocity = (_interactTransform.position + _interactTransform.forward * 1.5f - transform.position) * CubeVel;
            transform.rotation = Quaternion.Euler(0, _interactTransform.rotation.eulerAngles.y, 0);
        }*/
        
        private void SetObjectPosition()
        {
            if (!_isPickedUp.Value) return;
            _rigidbody.linearVelocity = (_interactTransform.position + _interactTransform.forward * 1.5f - transform.position) * CubeVel;
            transform.rotation = Quaternion.Euler(0, _interactTransform.rotation.eulerAngles.y, 0);
        }
        
        [Rpc(SendTo.Server)]
        private void SetTransformsServerRpc(NetworkBehaviourReference interactor, bool pickingUp = true)
        {
            if (!interactor.TryGet(out Player.Player player)) return;
            _isPickedUp.Value = pickingUp;
            _rigidbody.useGravity = !_isPickedUp.Value;
            _rigidbody.maxLinearVelocity = _isPickedUp.Value ? float.MaxValue : _rigidbody.maxLinearVelocity = 5f;
            _interactTransform = pickingUp ? player.GetInteractPoint() : transform;
        }

        #endregion

        #region Interface Methods

        public void PrimaryInteract(NetworkBehaviourReference interactor, bool pickingUp = true)
        {
            SetTransformsServerRpc(interactor, pickingUp);
        }

        public void SecondaryInteract(NetworkBehaviourReference interactor)
        {
        }

        public string GetInteractName()
        {
            return gameObject.name;
        }

        public bool IsPickedUp()
        {
            return _isPickedUp.Value;
        }

        #endregion
    }
}

