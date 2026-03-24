using System;
using PlayerScripts;
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

        private Vector3 _floorTransformLastFrame;
        
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
            else
            {
                if(Physics.Raycast(transform.position, Vector3.down, out var hitInfo, 1f))
                {
                    transform.position = hitInfo.point + hitInfo.distance * Vector3.up;;
                }
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
            }
            else
            {
                transform.SetParent(null);
            }
        }
        
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void SetTransformsServerRpc(NetworkBehaviourReference interactor, bool beingPickedUp = true)
        {
            _isInteractedWith.Value = beingPickedUp;
            
            if (!interactor.TryGet(out PlayerScripts.OwnerPlayer player)) return;
            SetParentClientRpc(interactor, beingPickedUp);
            
            if (beingPickedUp)
            {
                transform.localPosition = Vector3.zero;
            }
            else
            {
                var interactPoint = player.GetInteractPoint();
                var hitObjects = Physics.RaycastAll(interactPoint.position, interactPoint.forward, 3f, ~(1 << 11));
                Array.Sort(hitObjects, OwnerPlayer.CompareDistance);
                if(hitObjects.Length > 0) {
                    foreach (var hit in hitObjects) {
                        if (hit.collider.gameObject == gameObject) {
                            continue;
                        } 
                        transform.position = hit.point + Vector3.up * 0.2f;
                        return;
                    }
                }
                else {
                    transform.position = interactPoint.position + interactPoint.forward * 3f;
                }
                
            }
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

        public virtual string GetInteractName()
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

