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
        [SerializeField] private GameObject previewTables;
        [SerializeField] private AIManager aiManager;
        [SerializeField] private GameObject particle;

        protected override void Update()
        {
            previewTables.SetActive(!IsInteractedWith());

            base.Update();
        }

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
            if (!Physics.Raycast(transform.position, Vector3.down, out var hit, float.PositiveInfinity,
                    groundLayer)) return;
            MoveTablesRpc();
                
            aiManager.StartSpawningCustomers();
            NetworkObject.Despawn(false);
        }

        [Rpc(SendTo.Everyone)]
        private void MoveTablesRpc()
        {
            tables.transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            Instantiate(particle, transform.position, Quaternion.identity);
        }

        public override string GetInteractName()
        {
            return "\nPlace down and press E to setup tables";
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, new Vector3(9, 2, 9));
        }
    }
}