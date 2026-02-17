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
        private readonly NetworkVariable<Vector3> _networkPosition = new ();
        
        protected Rigidbody rb;
        protected Collider colli;
        private LayerMask originalExcludeLayer;

        #endregion

        #region Unity Methods

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody>();
            colli = GetComponent<Collider>();
        }
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _isInteractedWith.OnValueChanged += PickedUpChanged;
            _networkPosition.OnValueChanged += CheckForDistance;
            
            rb.useGravity = true;
            rb.isKinematic = false;
        }

        private void CheckForDistance(Vector3 previousValue, Vector3 newValue)
        {
            if (Vector3.Distance(_networkPosition.Value, transform.position) > 5f)
            {
                transform.position = newValue;
            }
        }

        private void Update()
        {
            if(_isInteractedWith.Value)
                transform.rotation = Quaternion.Euler(0, transform.parent.rotation.eulerAngles.y, 0);
            

            if (IsServer)
            {
                _networkPosition.Value = transform.position;
            }
        }

        private void PickedUpChanged(bool previousValue, bool newValue)
        {
            if (newValue == false)
            {
                transform.parent = null;
            }
        }

        #endregion
        
        #region RPC Methods
        
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void SetTransformsServerRpc(NetworkBehaviourReference interactor, bool pickingUp = true)
        {
            SetParentClientRpc(interactor, pickingUp);
            _isInteractedWith.Value = pickingUp;
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        private void SetParentClientRpc(NetworkBehaviourReference interactor, bool pickingUp)
        {
            rb.useGravity = !pickingUp;
            rb.isKinematic = pickingUp;
            if (interactor.TryGet(out Player.Player player))
            {
                transform.SetParent(player.GetInteractPoint());
                transform.position = player.GetInteractPoint().position + player.GetInteractPoint().forward * 2f;
            }
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

