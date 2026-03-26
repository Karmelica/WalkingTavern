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
        public static Dictionary<ulong, Transform> handTransforms = new();

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
            StartCoroutine(ScreenFadeout());
        }


        private void OnSceneLoadedEvent(string currentSceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            _isLoaded = true;
            
            if (!IsHost || scenesToSpawnPlayersIn.Count == 0) return;
            _spawnPos = Camera.main ? Camera.main.transform.position : Vector3.up;
            foreach (var sceneName in scenesToSpawnPlayersIn)
            {
                if (currentSceneName == sceneName) SpawnPlayers(clientsCompleted);
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

        private void SpawnPlayers(List<ulong> clients) 
        {
            foreach (var clientId in clients)
            {
                var playerInstance = Instantiate(playerPrefab, _spawnPos + Random.insideUnitSphere, Quaternion.identity);
                
                handTransforms.Add(clientId, playerInstance.GetComponent<OwnerPlayer>().GetHandPoint());
                
                playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
            }
        }
    }
}
