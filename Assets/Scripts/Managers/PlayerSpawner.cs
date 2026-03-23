using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Managers
{
    public class PlayerSpawner : NetworkBehaviour
    {
        [SerializeField] private GameObject loadingCanvas;
        [SerializeField] private GameObject blackScreen;
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private List<string> scenesToSpawnPlayersIn;
        private Vector3 _spawnPos;
        private bool _isLoaded;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public override void OnNetworkSpawn()
        {
            NetworkManager.SceneManager.OnLoadEventCompleted += OnSceneLoadedEvent;
            NetworkManager.SceneManager.OnLoad += OnSceneLoaded;
        }

        public override void OnNetworkDespawn()
        {
            NetworkManager.SceneManager.OnLoadEventCompleted -= OnSceneLoadedEvent;
            NetworkManager.SceneManager.OnLoad -= OnSceneLoaded;
        }

        private void OnSceneLoaded(ulong clientId, string sceneName, LoadSceneMode loadSceneMode, AsyncOperation asyncOperation)
        {
            _isLoaded = false;
            StartCoroutine(ScreenFadeout());
        }


        private void OnSceneLoadedEvent(string currentSceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            _isLoaded = true;
            if (!IsHost || scenesToSpawnPlayersIn.Count == 0) return;
            _spawnPos = Camera.main ? Camera.main.transform.position : Vector3.up;
            foreach (var sceneName in scenesToSpawnPlayersIn)
            {
                if(currentSceneName == sceneName) SpawnPlayers(clientsCompleted);
                return;
            }
            Debug.Log("No scene to spawn players found");
        }
        
        private IEnumerator ScreenFadeout()
        {
            loadingCanvas.SetActive(true);
            while (!_isLoaded)
            {
                yield return null;
            }

            yield return new WaitForSeconds(1f);
            loadingCanvas.SetActive(false);
        }

        private void SpawnPlayers(List<ulong> clients) 
        {
            foreach (var clientId in clients)
            {
                var playerInstance = Instantiate(playerPrefab, _spawnPos + Random.insideUnitSphere, Quaternion.identity);
                playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
            }
        }
    }
}
