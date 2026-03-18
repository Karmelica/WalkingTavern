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
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            _rigidbody.useGravity = true;
            _rigidbody.isKinematic = false;
        }
        

        private void Update()
        {
            if(_isInteractedWith.Value)
            {
                transform.position = transform.parent.position;
            }
        }

        #endregion
        
        #region RPC Methods
        
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void PlaceOnMinigameRpc()
        {
            _rigidbody.useGravity = false;
            _rigidbody.isKinematic = true;
        }
        
        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        private void SetParentClientRpc(NetworkBehaviourReference interactor, bool beingPickedUp)
        {
            _rigidbody.useGravity = !beingPickedUp;
            _rigidbody.isKinematic = beingPickedUp;

            if (interactor.Equals(null))
            {
                transform.SetParent(null);
            }

            if (!interactor.TryGet(out PlayerScripts.OwnerPlayer player)) return;
            
            if (beingPickedUp)
            {
                transform.SetParent(player.GetHandPoint());
                transform.position = Vector3.zero;
            }
            else
            {
                var interactPoint = player.GetInteractPoint();
                if (Physics.Raycast(interactPoint.position, interactPoint.forward, out var hit, 2f, ~(1<<11)))
                {
                    transform.position = hit.point + Vector3.up * 0.2f;
                }
                else
                {
                    transform.position = interactPoint.position + interactPoint.forward * 2f;
                }
                transform.SetParent(null);
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

