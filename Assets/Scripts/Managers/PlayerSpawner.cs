using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using Netcode.Transports.Facepunch;
using PlayerScripts;
using Steamworks;
using Steamworks.Data;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Color = UnityEngine.Color;
using Image = UnityEngine.UI.Image;
using Random = UnityEngine.Random;

namespace Managers
{
    public class PlayerSpawner : NetworkBehaviour, ISocketManager
    {
        private Dictionary<ulong, NetworkObject> _clientIdToPlayerObject = new();
        [SerializeField] private GameObject loadingCanvas;
        [SerializeField] private Image blackScreen;
        [SerializeField] private GameObject playerPrefab;
        [Scene]
        [SerializeField] private List<string> scenesToSpawnPlayersIn;
        private Vector3 _spawnPos;
        private bool _isLoaded;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public override void OnNetworkSpawn()
        {
            NetworkManager.SceneManager.OnLoad += OnSceneLoadStarted;
            NetworkManager.SceneManager.OnLoadComplete += OnSceneLoaded;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        public override void OnNetworkDespawn()
        {
            NetworkManager.SceneManager.OnLoad -= OnSceneLoadStarted;
            NetworkManager.SceneManager.OnLoadComplete -= OnSceneLoaded;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        private void OnSceneLoadStarted(ulong clientId, string currentSceneName, LoadSceneMode loadSceneMode, AsyncOperation asyncOperation)
        {
            StartCoroutine(ScreenFadeout());
        }

        private void OnSceneLoaded(ulong clientId, string currentSceneName, LoadSceneMode loadSceneMode)
        {
            _isLoaded = true;

            if (clientId != NetworkManager.Singleton.LocalClientId) return;
            if (scenesToSpawnPlayersIn.Count == 0) throw new Exception("Scenes to spawn players in not set");
            foreach (var scene in scenesToSpawnPlayersIn)
            {
                if (currentSceneName != scene) continue;
                _spawnPos = Camera.main ? Camera.main.transform.position : Vector3.up;
                if (currentSceneName == scene) RequestSpawnPlayerRpc();
                return;
            }
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
        }
        
        private void OnClientDisconnected(ulong clientId)
        {
            //if (!IsServer) return;
            //if (!_clientIdToPlayerObject.Remove(clientId, out var networkObject)) return;
            //if (networkObject.IsSpawned)
            //{
            //    networkObject.Despawn();
            //}
        }

        public void OnConnecting(Connection connection, ConnectionInfo info)
        {
            Debug.Log(info);
        }

        public void OnConnected(Connection connection, ConnectionInfo info)
        {
            Debug.Log(info);
        }

        public void OnDisconnected(Connection connection, ConnectionInfo info)
        {
            Debug.Log(info);
        }

        public void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime,
            int channel)
        {
            
        }
    }
}
