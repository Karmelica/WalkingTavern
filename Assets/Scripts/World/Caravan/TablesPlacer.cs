using System;
using System.Linq;
using Managers;
using MyInterfaces;
using NaughtyAttributes;
using PlayerScripts;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace World.Caravan
{
    public class TablesPlacer : MoveableObject
    {
        [Layer]
        [SerializeField] private int groundLayer;
        [SerializeField] private GameObject tables;
        [SerializeField] private GameObject previewTables;
        [SerializeField] private AIManager aiManager;
        [SerializeField] private GameObject particle;
        [SerializeField] private TableCollisionTest[] colliders;
        
        protected override void Update()
        {
            previewTables.SetActive(!transform.parent);

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
            if (colliders.Any(table => table.IsColliding)) return;
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

        public override string GetInteractText()
        {
            return $"Setup tables";
        }
    }
}