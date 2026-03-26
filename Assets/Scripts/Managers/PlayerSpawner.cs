using System;
using System.Collections;
using System.Collections.Generic;
using PlayerScripts;
using Steamworks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;
using Random = UnityEngine.Random;

namespace Managers
{
    public class PlayerSpawner : NetworkBehaviour
    {
        private Dictionary<ulong, NetworkObject> _clientIdToPlayerObject = new();
        [SerializeField] private GameObject loadingCanvas;
        [SerializeField] private Image blackScreen;
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
            NetworkManager.SceneManager.OnLoadComplete += OnSceneLoaded;
            NetworkManager.SceneManager.OnLoad += OnSceneLoadStarted;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }



        public override void OnNetworkDespawn()
        {
            NetworkManager.SceneManager.OnLoadComplete -= OnSceneLoaded;
            NetworkManager.SceneManager.OnLoad -= OnSceneLoadStarted;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        private void OnSceneLoadStarted(ulong clientId, string currentSceneName, LoadSceneMode loadSceneMode, AsyncOperation asyncOperation)
        {
            StartCoroutine(ScreenFadeout());
        }

        private void OnSceneLoaded(ulong clientId, string currentSceneName, LoadSceneMode loadSceneMode)
        {
            _isLoaded = true;
            
            if (scenesToSpawnPlayersIn.Count == 0) return;
            _spawnPos = Camera.main ? Camera.main.transform.position : Vector3.up;
            foreach (var sceneName in scenesToSpawnPlayersIn)
            {
                if (currentSceneName == sceneName) RequestSpawnPlayerRpc();
                return;
            }
            Debug.Log("No scene to spawn players found");
        }
        
        private IEnumerator ScreenFadeout()
        {
            _isLoaded = false;
            blackScreen.color = Color.black;
            loadingCanvas.SetActive(true);
            while (!_isLoaded)
            {
                yield return null;
            }
            yield return new WaitForSeconds(0.5f);
            
            while (blackScreen.color.a > 0)
            {
                var colorA = blackScreen.color;
                colorA.a -= 0.01f;
                blackScreen.color = colorA;
                yield return new WaitForSeconds(0.01f);
            }
            loadingCanvas.SetActive(false);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void RequestSpawnPlayerRpc(RpcParams rpcParams = default)
        {
            var clientId = rpcParams.Receive.SenderClientId;

            var playerInstance = Instantiate(playerPrefab, _spawnPos + Random.insideUnitSphere, Quaternion.identity);
            var networkObject = playerInstance.GetComponent<NetworkObject>();
            networkObject.SpawnAsPlayerObject(clientId, true);
            
            _clientIdToPlayerObject.TryAdd(clientId, networkObject);
        }
        
        private void OnClientDisconnected(ulong clientId)
        {
            if (!IsServer) return;
            if (!_clientIdToPlayerObject.TryGetValue(clientId, out var networkObject)) return;
            if (networkObject.IsSpawned)
            {
                networkObject.Despawn();
            }
            if (networkObject.gameObject != null)
            {
                Destroy(networkObject.gameObject);
            }
            
            _clientIdToPlayerObject.Remove(clientId);
                
        }
    }
}
