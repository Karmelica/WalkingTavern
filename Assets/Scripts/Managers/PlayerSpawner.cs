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
        private Dictionary<ulong, NetworkObject> _steamToPlayerObject = new();
        private Dictionary<ulong, ulong> _ClientIdToSteamId = new();
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
            
            ulong steamId = 0;

            if (SteamClient.IsValid)
            {
                steamId = SteamClient.SteamId.Value;
            }
            
            if (scenesToSpawnPlayersIn.Count == 0) return;
            _spawnPos = Camera.main ? Camera.main.transform.position : Vector3.up;
            foreach (var sceneName in scenesToSpawnPlayersIn)
            {
                if (currentSceneName == sceneName) RequestSpawnPlayerRpc(steamId);
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
        private void RequestSpawnPlayerRpc(ulong steamId, RpcParams rpcParams = default)
        {
            var clientId = rpcParams.Receive.SenderClientId;

            if (steamId != 0 && _steamToPlayerObject.TryGetValue(steamId, out var playerObject) && playerObject != null &&
                playerObject.IsSpawned)
            {
                playerObject.ChangeOwnership(clientId);
                _ClientIdToSteamId[clientId] = steamId;
                _clientIdToPlayerObject[clientId] =  playerObject;
                _steamToPlayerObject[steamId] = playerObject;
                return;
            }
            
            var playerInstance = Instantiate(playerPrefab, _spawnPos + Random.insideUnitSphere, Quaternion.identity);
            var networkObject = playerInstance.GetComponent<NetworkObject>();
            networkObject.SpawnAsPlayerObject(clientId, true);
            
            _ClientIdToSteamId.TryAdd(clientId, steamId);
            _clientIdToPlayerObject.TryAdd(clientId, networkObject);
            if(steamId != 0) _steamToPlayerObject.TryAdd(steamId, networkObject);
        }
        
        private void OnClientDisconnected(ulong clientId)
        {
            if (!_ClientIdToSteamId.TryGetValue(clientId, out var steamId)) return;
            if (!_clientIdToPlayerObject.TryGetValue(steamId, out var networkObject)) return;
            
            if (networkObject.IsSpawned)
            {
                networkObject.Despawn();
            }
            if (networkObject.gameObject != null)
            {
                Destroy(networkObject.gameObject);
            }
            if (steamId != 0)
            {
                _steamToPlayerObject.Remove(steamId);
            }
            
            _clientIdToPlayerObject.Remove(clientId);
            _ClientIdToSteamId.Remove(clientId);
                
        }
    }
}
