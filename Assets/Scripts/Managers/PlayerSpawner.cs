using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Managers
{
    public class PlayerSpawner : NetworkBehaviour
    {
        [SerializeField] private GameObject playerPrefab;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public override void OnNetworkSpawn()
        {
            NetworkManager.SceneManager.OnLoadEventCompleted += OnSceneLoadedEvent;
        }

        private void OnSceneLoadedEvent(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            if(IsHost && sceneName == "SampleScene") SpawnPlayers(clientsCompleted);
        }

        private void SpawnPlayers(List<ulong> clients) 
        {
            foreach (var clientId in clients)
            {
                var playerInstance = Instantiate(playerPrefab, new Vector3(Random.Range(0f, 5), 1, Random.Range(0f, 5)), Quaternion.identity);
                playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
            }
        }
    }
}
