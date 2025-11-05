using System.Collections.Generic;
using Unity.Netcode;
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

        private void Start()
        {
            if (NetworkManager == null) return;
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnectCallback;
            NetworkManager.OnClientConnectedCallback += (ulong clientId) =>
            {
                Debug.Log($"Client connected: {clientId}");
            };
        }

        private static void OnClientDisconnectCallback(ulong obj)
        {
            if (!NetworkManager.IsServer && NetworkManager.DisconnectReason != string.Empty)
            {
                Debug.Log($"Approval Declined Reason: {NetworkManager.DisconnectReason}");
            }
        }
    }
}