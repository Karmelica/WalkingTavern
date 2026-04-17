using System;
using Managers;
using NaughtyAttributes;
using PlayerScripts;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace World.Caravan
{
    [RequireComponent(typeof(Collider), typeof(NetworkTransform))]
    public class TablesPlacer : MoveableObject
    {
        [Layer]
        [SerializeField] private int groundLayer;
        [SerializeField] private GameObject tables;
        [SerializeField] private AIManager aiManager;
        
        
        
        public override IInteractable SecondaryInteract(OwnerPlayer interactor)
        {
            UnpackTablesRpc();
            return null;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            gameObject.SetActive(false);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void UnpackTablesRpc()
        {
            if(Physics.Raycast(transform.position, Vector3.down, out var hit, float.PositiveInfinity, groundLayer))
            {
                MoveTablesRpc();
                
                aiManager.StartSpawningCustomers();
                NetworkObject.Despawn(false);
            }
        }

        [Rpc(SendTo.Everyone)]
        private void MoveTablesRpc()
        {
            tables.transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        }

        public override string GetInteractName()
        {
            return "\nPlace down and press E to setup tables";
        }
    }
}