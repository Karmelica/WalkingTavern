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
        
        private Transform _interactTransform;
        private readonly NetworkVariable<bool> _isInteractedWith = new (false);
        
        private Rigidbody _rigidbody;

        #endregion

        #region Unity Methods

        protected virtual void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            _rigidbody.useGravity = !transform.parent;
            if(_isInteractedWith.Value && transform.parent)
            {
                transform.position = transform.parent.position;
            }
        }
        
        public void PlaceOnMinigame()
        {
            _rigidbody.linearDamping = Single.PositiveInfinity;
            _rigidbody.linearDamping = 0.1f;
            transform.rotation = Quaternion.identity;
        }

        #endregion
        
        #region RPC Methods
        
        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        private void SetParentClientRpc(NetworkBehaviourReference interactor, bool beingPickedUp)
        {
            if (!interactor.TryGet(out PlayerScripts.OwnerPlayer player)) return;
            
            if (beingPickedUp)
            {
                transform.SetParent(player.GetHandPoint());
                transform.position = Vector3.zero;
            }
            else
            {
                var interactPoint = player.GetInteractPoint();
                transform.SetParent(null);
                if (Physics.Raycast(interactPoint.position, interactPoint.forward, out var hit, 3f, ~(1<<11)))
                {
                    transform.position = hit.point + Vector3.up * 0.2f;
                }
                else
                {
                    transform.position = interactPoint.position + interactPoint.forward * 3f;
                }
            }
        }
        
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void SetTransformsServerRpc(NetworkBehaviourReference interactor, bool beingPickedUp = true)
        {
            SetParentClientRpc(interactor, beingPickedUp);
            _isInteractedWith.Value = beingPickedUp;
        }

        #endregion

        #region Interface Methods

        public IInteractable PrimaryInteract(NetworkBehaviourReference interactor, bool beingPickedUp = true)
        {
            SetTransformsServerRpc(interactor, beingPickedUp);
            return this;
        }

        public IInteractable SecondaryInteract(NetworkBehaviourReference interactor)
        {
            return null;
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

