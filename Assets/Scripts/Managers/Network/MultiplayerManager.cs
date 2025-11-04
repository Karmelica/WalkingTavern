using System;
using System.Collections.Generic;
using Netcode.Transports.Facepunch;
using Steamworks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers.Network
{
    public class MultiplayerManager : MonoBehaviour
    {
        [SerializeField] private GameObject loginUI;
        
        private TMP_InputField clientSteamIdInputField;

        void Awake()
        {
            clientSteamIdInputField = GetComponentInChildren<TMP_InputField>();
        }

        public void OnHostButtonClicked()
        {
            loginUI.SetActive(false);
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
            NetworkManager.Singleton.StartHost();
            if (!NetworkManager.Singleton.IsServer) return;
            NetworkManager.Singleton.SceneManager.LoadScene("SampleScene", LoadSceneMode.Single); 
            Debug.Log("Host started with SteamID: " + SteamClient.SteamId);
        }

        public void OnClientButtonClicked()
        {
            if (string.IsNullOrEmpty(clientSteamIdInputField.text))
            {
                Debug.LogWarning("Client Steam ID input field is empty.");
                return;
            }

            loginUI.SetActive(false);
            var steamId = clientSteamIdInputField.text;
            var targetSteamId = ulong.Parse(steamId);
            var facepunchTransport = NetworkManager.Singleton.GetComponent<FacepunchTransport>();
            facepunchTransport.targetSteamId = targetSteamId;
            NetworkManager.Singleton.StartClient();
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

            response.Position = Vector3.zero + new Vector3(0,5,0);
            response.Pending = false;
        }
    }
}