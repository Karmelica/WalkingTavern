using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class PickupsGenerator : NetworkBehaviour
{
    [SerializeField] private GameObject[] pickupPrefab;
    private List<NetworkObject> _spawnedObjects = new List<NetworkObject>();
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer) return;
        foreach (var item in pickupPrefab)
        {
            var random = Random.Range(5, 35);
            for (var i = 0; i < random; i++){
                var pos = new Vector3(Random.Range(-20, 20), 1, Random.Range(-100, 80));
                var prefab = Instantiate(item, pos, Quaternion.identity);
                var netObj = prefab.GetComponent<NetworkObject>();
                _spawnedObjects.Add(netObj);
                netObj.Spawn();
            }
        }
    }

    private void OnDisable()
    {
        if (!IsServer) return;
        foreach (var netObj in _spawnedObjects)
        {
            if(netObj.IsSpawned)
                netObj.Despawn();
        }
    }
}
