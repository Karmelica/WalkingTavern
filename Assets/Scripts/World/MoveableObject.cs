using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using PlayerScripts;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace World
{
    [RequireComponent(typeof(Collider), typeof(NetworkTransform))]
    public class MoveableObject : NetworkBehaviour, IInteractable
    {
        #region Variables

        public bool isOnMinigame;
        private Collider _collider;
        private readonly NetworkVariable<bool> _isInteractedWith = new ();

        #endregion

        #region Unity Methods

        protected virtual void Awake()
        {
            _collider = GetComponent<Collider>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            NetworkObject.DontDestroyWithOwner = true;
        }

        protected virtual void Update()
        {
            if (!IsOwner) return;
            if (transform.parent)
            {
                transform.position = transform.parent.position;
                transform.up = Vector3.up;
            }
            else
            {
                if (isOnMinigame) return;
                Physics.Raycast(transform.position, Vector3.down, out var hit, Single.PositiveInfinity, ~(1 << 2),
                    QueryTriggerInteraction.Ignore);
                transform.up = hit.normal;
                transform.position = hit.point + transform.up * _collider.bounds.extents.y;
            }

            Physics.SyncTransforms();
        }

        #endregion
        
        #region RPC Methods
        
        [Rpc(SendTo.Owner, InvokePermission = RpcInvokePermission.Everyone)]
        public void PlaceDownRpc()
        {
            Physics.Raycast(transform.position, Vector3.down, out var hit, Single.PositiveInfinity, ~(1 << 2),
                QueryTriggerInteraction.Ignore);
            transform.up = hit.normal;
            transform.position = hit.point + transform.up * _collider.bounds.extents.y;
        }
        
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void ChangeOwnerServerRpc(bool startedInteraction = true, RpcParams rpcParams = default)
        {
            _isInteractedWith.Value = startedInteraction;
            
            if (!startedInteraction) return;
            var clientId = rpcParams.Receive.SenderClientId;
            NetworkObject.ChangeOwnership(clientId);
        }

        public void MoveOnMinigame(Vector3 position)
        {
            MoveOnMinigameRpc(position);
        }

        [Rpc(SendTo.Owner, InvokePermission = RpcInvokePermission.Everyone)]
        private void MoveOnMinigameRpc(Vector3 position)
        {
            transform.position = position;
            Physics.SyncTransforms();
        }

        #endregion

        #region Interface Methods

        public IInteractable PrimaryInteract(OwnerPlayer interactor, bool startedInteraction = true,
            bool disconnection = false)
        {
            if (disconnection)
            {
                transform.SetParent(null);
                if(NetworkManager.Singleton)
                {
                    OwnerDisconnectedRpc();
                }
                return null;
            }
            
            Vector3 placePoint = Vector3.zero;
            Vector3 placeRotation = Vector3.up;
            
            if (startedInteraction) {
                ChangeOwnerServerRpc(true);
                transform.SetParent(interactor.GetHandPoint());
            }
            else {
                ChangeOwnerServerRpc(false);
                transform.SetParent(null);
                var interactPoint = interactor.GetInteractPoint();
                var hitObjects = Physics.RaycastAll(interactPoint.position, interactPoint.forward, 3f, ~(1 << 11), QueryTriggerInteraction.Ignore);
                if (hitObjects.Length > 0) {
                    Array.Sort(hitObjects, Utilis.CompareRaycastDistance);
                    foreach (var hit in hitObjects) {
                        if (hit.collider.gameObject == gameObject) {
                            continue;
                        }

                        placePoint = hit.point + hit.normal * 0.2f;
                        placeRotation = hit.normal;
                        return TargetLocation(placeRotation, placePoint);
                    }
                }
                
                //drop on ground if nothing or only held object was hit
                Physics.Raycast(transform.position, Vector3.down, out var groundHit, Single.PositiveInfinity, ~(1<<2), QueryTriggerInteraction.Ignore);
                placeRotation = groundHit.normal;
                placePoint = groundHit.point;
            }

            return TargetLocation(placeRotation, placePoint);

            IInteractable TargetLocation(Vector3 vector3, Vector3 placePoint1)
            {
                transform.up = vector3;
                transform.position = placePoint1 + transform.up * _collider.bounds.extents.y;
                Physics.SyncTransforms();
                return this;
            }
        }

        [Rpc(SendTo.Server)]
        private void OwnerDisconnectedRpc()
        {
            _isInteractedWith.Value = false;
        }

        public virtual IInteractable SecondaryInteract(OwnerPlayer interactor)
        {
            return null;
        }

        public virtual string GetInteractText()
        {
            return $"Pick up {gameObject.name}";
        }

        public bool IsInteractedWith()
        {
            return _isInteractedWith.Value;
        }

        #endregion

    }
}

