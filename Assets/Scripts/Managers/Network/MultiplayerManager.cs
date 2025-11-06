using System.Collections;
using System.Collections.Generic;
using Netcode.Transports.Facepunch;
using Steamworks;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers.Network
{
    public class MultiplayerManager : MonoBehaviour
    {
        [SerializeField] private GameObject loginUI;
        private static NetworkManager NetworkManager => NetworkManager.Singleton;
        
        private TMP_InputField _clientSteamIdInputField;

        void Awake()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            _clientSteamIdInputField = GetComponentInChildren<TMP_InputField>();
        }

        public void OnHostButtonClicked()
        {
            loginUI.SetActive(false);
            NetworkManager.ConnectionApprovalCallback += ApprovalCheck;
            NetworkManager.StartHost();
            if (!NetworkManager.IsServer) return;
            LoadGame();
        }

        private static void LoadGame()
        {
            NetworkManager.SceneManager.OnLoadEventCompleted += SceneManagerOnOnLoadEventCompleted;
            NetworkManager.SceneManager.LoadScene("SampleScene", LoadSceneMode.Additive);
            Debug.Log("Host started with SteamID: " + SteamClient.SteamId);
        }

        private static void SceneManagerOnOnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            NetworkManager.SceneManager.OnLoadEventCompleted -= SceneManagerOnOnLoadEventCompleted;
            Debug.Log($"Scene {sceneName} loaded for all clients.");
        }

        public void OnClientButtonClicked()
        {
            if (string.IsNullOrEmpty(_clientSteamIdInputField.text))
            {
                Debug.LogWarning("Client Steam ID input field is empty.");
                return;
            }

            loginUI.SetActive(false);
            var steamId = _clientSteamIdInputField.text;
            var targetSteamId = ulong.Parse(steamId);
            var facepunchTransport = NetworkManager.Singleton.GetComponent<FacepunchTransport>();
            facepunchTransport.targetSteamId = targetSteamId;
            NetworkManager.Singleton.StartClient();
        }

        public void DisconnectClient(ulong clientId)
        {
            if (NetworkManager.IsServer)
            {
                NetworkManager.DisconnectClient(clientId);
            }
        }

        private static void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            if(NetworkManager.Singleton.ConnectedClients.Count >= 4)
            {
                response.Approved = false;
                response.Reason = "Server is full";
                response.Pending = false;
                return;
            }
            
            response.Approved = true;
            response.CreatePlayerObject = true;

            response.PlayerPrefabHash = 1959477017;

            response.Position = Vector3.zero + new Vector3(0, 1, 0);
            response.Pending = false;
        }
    }
}