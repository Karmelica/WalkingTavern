using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Managers
{
	public class PickupsGenerator : NetworkBehaviour
	{
		[SerializeField] private GameObject[] pickupPrefab;
		private readonly List<NetworkObject> _spawnedObjects = new();

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			if (!IsServer) return;
			SpawnObjects();
		}
		
		private async void SpawnObjects()
		{
			try {
				await Awaitable.WaitForSecondsAsync(0.1f);
				foreach (var item in pickupPrefab) {
					for (var i = 0; i < 35; i++) {
						var pos = new Vector3(Random.Range(-20, 20), 1, Random.Range(-100, 80));
						var prefab = Instantiate(item, pos, Quaternion.identity);
						var netObj = prefab.GetComponent<NetworkObject>();
						_spawnedObjects.Add(netObj);
						netObj.Spawn(true);
					}
				}
			} catch (Exception e) {
				Debug.LogException(e);
			}
		}

		public override void OnNetworkDespawn()
		{
			base.OnNetworkDespawn();
			if (!IsServer) return;
			foreach (var netObj in _spawnedObjects) {
				if (netObj.IsSpawned) {
					netObj.Despawn();
				}
			}
		}
	}
}