using System;
using Netcode.Transports.Facepunch;
using Steamworks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Managers.Network
{
    public class MultiplayerManager : MonoBehaviour
    {
        [SerializeField] private GameObject loginUI;

        private static FacepunchTransport _facepunchTransport;
        private static UnityTransport _unityTransport;
        
#if UNITY_EDITOR
        void Awake()
        {
            _facepunchTransport = NetworkManager.Singleton.GetComponent<FacepunchTransport>();
            _unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (_facepunchTransport != null && _unityTransport != null)
            {
                _facepunchTransport.enabled = false;
                _unityTransport.enabled = true;
                NetworkManager.Singleton.NetworkConfig.NetworkTransport = _unityTransport;
                Debug.Log("Using Unity Transport for Editor");
            }
        }
        
        public void OnHostButtonClicked()
        {
            loginUI.SetActive(false);
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
            NetworkManager.Singleton.StartHost();
        }
        
        public void OnClientButtonClicked()
        {
            loginUI.SetActive(false);
            NetworkManager.Singleton.StartClient();
        }
        
#else
        private TMP_InputField clientSteamIdInputField;


        void Awake()
        {
            _facepunchTransport = NetworkManager.Singleton.GetComponent<FacepunchTransport>();
            _unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (_facepunchTransport != null && _unityTransport != null)
            {
                _unityTransport.enabled = false;
                _facepunchTransport.enabled = true;

                NetworkManager.Singleton.NetworkConfig.NetworkTransport = _facepunchTransport;
                Debug.Log("Using Facepunch Transport for Build");
            }
            clientSteamIdInputField = GetComponentInChildren<TMP_InputField>();
        }

        public void OnHostButtonClicked()
        {
            loginUI.SetActive(false);
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
            NetworkManager.Singleton.StartHost();
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
            _facepunchTransport.targetSteamId = targetSteamId;
            NetworkManager.Singleton.StartClient();
        }
#endif


        
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

            response.PlayerPrefabHash = null;

            response.Position = Vector3.zero + new Vector3(0,5,0);
            response.Pending = false;
        }
    }
}