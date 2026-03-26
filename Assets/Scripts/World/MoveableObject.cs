using System;
using System.Collections.Generic;
using Managers;
using PlayerScripts;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace World
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(NetworkTransform))]
    
    public class MoveableObject : NetworkBehaviour, IInteractable
    {
        #region Variables
        
        private Collider _collider;
        
        private readonly NetworkVariable<bool> _isInteractedWith = new (false);

        #endregion

        #region Unity Methods

        protected virtual void Awake()
        {
            _collider = GetComponent<Collider>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _isInteractedWith.OnValueChanged += OnInteractedValueChanged;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _isInteractedWith.OnValueChanged -= OnInteractedValueChanged;
        }

        private void OnInteractedValueChanged(bool previousValue, bool newValue)
        {
            _collider.enabled = !newValue;
        }

        private void Update()
        {
            if (!IsServer) return;
            if(transform.parent) {
                transform.position = transform.parent.position;
                transform.rotation = transform.parent.rotation;
            }
            else
            {
                transform.rotation = Quaternion.identity;
            }
        }

        public void PlaceOnMinigame()
        {
            transform.rotation = Quaternion.identity;
        }

        #endregion
        
        #region RPC Methods
        
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void SetTransformsServerRpc(Vector3 placePoint, bool startedInteraction = true, RpcParams rpcParams = default)
        {
            ulong clientId = rpcParams.Receive.SenderClientId;
            _isInteractedWith.Value = startedInteraction;

            transform.SetParent(startedInteraction ? PlayerSpawner.handTransforms[clientId] : null);

            transform.localPosition = placePoint;
        }

        #endregion

        #region Interface Methods

        public IInteractable PrimaryInteract(OwnerPlayer interactor, bool startedInteraction = true)
        {
            Vector3 placePoint = Vector3.zero;
            
            if (startedInteraction)
            {
                SetTransformsServerRpc(placePoint, true);
            }
            else
            {
                var interactPoint = interactor.GetInteractPoint();
                var hitObjects = Physics.RaycastAll(interactPoint.position, interactPoint.forward, 3f, ~(1 << 11), QueryTriggerInteraction.Ignore);
                Array.Sort(hitObjects, OwnerPlayer.CompareDistance);
                if (hitObjects.Length > 0) {
                    foreach (var hit in hitObjects) {
                        if (hit.collider.gameObject == gameObject) {
                            continue;
                        }

                        if(Mathf.Abs(hit.normal.y) > 0.5) {
                            placePoint = hit.point + Vector3.up * 0.1f;
                            SetTransformsServerRpc(placePoint, false);
                            return this;
                        }
                        Physics.Raycast(hit.point + hit.normal * 0.2f, Vector3.down, out var wallHit, Single.PositiveInfinity, ~(1<<2), QueryTriggerInteraction.Ignore);
                        placePoint = wallHit.point + Vector3.up * 0.1f;
                        SetTransformsServerRpc(placePoint, false);
                        return this;
                        
                    }
                }
                Physics.Raycast(transform.position, Vector3.down, out var groundHit, Single.PositiveInfinity, ~(1<<2), QueryTriggerInteraction.Ignore);
                placePoint = groundHit.point +  Vector3.up * 0.1f;
                SetTransformsServerRpc(placePoint, false);
            }
            
            return this;
        }

        public IInteractable SecondaryInteract(OwnerPlayer interactor)
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

