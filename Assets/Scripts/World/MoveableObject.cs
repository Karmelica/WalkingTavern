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
        private readonly NetworkVariable<bool> _isInteractedWith = new (false);
        
        protected Rigidbody rb;
        protected Collider colli;
        private LayerMask originalExcludeLayer;

        #endregion

        #region Unity Methods

        protected virtual void Update()
        {
            //SetObjectPositionServerRpc();
            SetObjectPosition();
        }

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody>();
            colli = GetComponent<Collider>();
            _interactTransform = transform;
            originalExcludeLayer = colli.excludeLayers.value;
        }

        #endregion
        
        #region RPC Methods
        
        private void SetObjectPosition()
        {
            if (!_isInteractedWith.Value) return;
            rb.linearVelocity = (_interactTransform.position + _interactTransform.forward * 1.5f - transform.position) * CubeVel;
            transform.rotation = Quaternion.Euler(0, _interactTransform.rotation.eulerAngles.y, 0);
        }
        
        [Rpc(SendTo.Server)]
        private void SetTransformsServerRpc(NetworkBehaviourReference interactor, bool pickingUp = true)
        {
            if (!interactor.TryGet(out Player.Player player)) return;
            _isInteractedWith.Value = pickingUp;
            rb.useGravity = !_isInteractedWith.Value;
            rb.maxLinearVelocity = _isInteractedWith.Value ? float.MaxValue : rb.maxLinearVelocity = 5f;
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

        public bool IsInteractedWith()
        {
            return _isInteractedWith.Value;
        }

        #endregion
    }
}

