using System;
using System.Collections;
using System.Collections.Generic;
using PlayerScripts;
using Steamworks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Managers
{
    public class PlayerSpawner : NetworkBehaviour
    {
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
        }


        public override void OnNetworkDespawn()
        {
            NetworkManager.SceneManager.OnLoadComplete -= OnSceneLoaded;
            NetworkManager.SceneManager.OnLoad -= OnSceneLoadStarted;
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
                if (currentSceneName == sceneName) RequestSpawnPlayerRpc(clientId);
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
        private void RequestSpawnPlayerRpc(ulong clientId) 
        {
            var playerInstance = Instantiate(playerPrefab, _spawnPos + Random.insideUnitSphere, Quaternion.identity);
            playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
            
        }
    }
}
