using System.Collections.Generic;
using Steamworks;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers.Network
{
    /// <summary>
    /// Connection Approval Handler Component
    /// </summary>
    /// <remarks>
    /// This should be placed on the same GameObject as the NetworkManager.
    /// It automatically declines the client connection for example purposes.
    /// </remarks>
    public class ConnectionApprovalHandler : MonoBehaviour
    {
        private static NetworkManager NetworkManager => NetworkManager.Singleton;

        private void OnEnable()
        {
            if (NetworkManager == null) return;
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnectCallback;
            NetworkManager.OnClientConnectedCallback += OnClientConnectionCallback;
        }

        private static void OnClientConnectionCallback(ulong clientId)
        {
                Debug.Log($"Client connected: {clientId}");
        }

        private void OnDisable()
        {
            if (NetworkManager == null) return;
            NetworkManager.OnClientDisconnectCallback -= OnClientDisconnectCallback;
            NetworkManager.OnClientConnectedCallback -= OnClientConnectionCallback;
        }

        private static void OnClientDisconnectCallback(ulong obj)
        {
            if (!NetworkManager.IsServer && NetworkManager.DisconnectReason != string.Empty)
            {
                Debug.Log($"{obj} Disconnected: {NetworkManager.DisconnectReason}");
            }

            if(NetworkManager.IsClient){
                SceneManager.LoadScene("Menu", LoadSceneMode.Single);
            }
            SteamClient.Shutdown();
        }
    }
}