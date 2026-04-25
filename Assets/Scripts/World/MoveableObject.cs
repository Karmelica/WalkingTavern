using System;
using MyInterfaces;
using NaughtyAttributes;
using PlayerScripts;
using Unity.Netcode;
using UnityEngine;

namespace World
{
    [RequireComponent(typeof(Collider))]
    public class MoveableObject : NetworkBehaviour, IInteractable, IObjectID
    {
        #region Variables

        [field: SerializeField] public uint ID { get; private set; }

        public bool isOnMinigame;
        private Collider _collider;

        #endregion

        #region Unity Methods
        
        protected virtual void Awake()
        {
            _collider = GetComponent<Collider>();
        }

        protected virtual void Update()
        {
            if (transform.parent || isOnMinigame) return;
            Physics.Raycast(transform.position, Vector3.down, out var hit, Single.PositiveInfinity, ~(1 << 2),
                QueryTriggerInteraction.Ignore);
            transform.up = hit.normal;
            transform.position = hit.point + transform.up * _collider.bounds.extents.y;
        }

        #endregion
        
        #region RPC Methods
        
        public void PlaceDown()
        {
            Physics.Raycast(transform.position, Vector3.down, out var hit, Single.PositiveInfinity, ~(1 << 2),
                QueryTriggerInteraction.Ignore);
            transform.up = hit.normal;
            transform.position = hit.point + transform.up * _collider.bounds.extents.y;
        }
        
        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
        private void SetObjectActiveRpc(bool setActive, Vector3 placePosition)
        {
            transform.position = placePosition;
            gameObject.SetActive(setActive);
        }

        public void MoveOnMinigame(Vector3 position)
        {
            transform.position = position;
        }

        #endregion

        #region Interface Methods

        public IInteractable PickupOrDropObject(bool pickUp, Vector3 placePosition)
        {
            SetObjectActiveRpc(!pickUp, placePosition);
            return this;
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
            return false;
        }

        #endregion

    }
}

