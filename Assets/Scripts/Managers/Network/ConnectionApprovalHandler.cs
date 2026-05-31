using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers.Network
{
	/// <summary>
	///     Connection Approval Handler Component
	/// </summary>
	/// <remarks>
	///     This should be placed on the same GameObject as the NetworkManager.
	///     It automatically declines the client connection for example purposes.
	/// </remarks>
	public class ConnectionApprovalHandler : MonoBehaviour
	{
		[SerializeField] private NetworkManager networkManager;

		private void OnEnable()
		{
			if (networkManager == null) return;
			networkManager.OnClientDisconnectCallback += OnClientDisconnectCallback;
			networkManager.OnClientConnectedCallback += OnClientConnectionCallback;
		}

		private void OnDisable()
		{
			if (networkManager == null) return;
			networkManager.OnClientDisconnectCallback -= OnClientDisconnectCallback;
			networkManager.OnClientConnectedCallback -= OnClientConnectionCallback;
		}

		private void OnClientConnectionCallback(ulong clientId)
		{
			Debug.Log($"Client connected: {clientId}");
		}

		private void OnClientDisconnectCallback(ulong clientId)
		{
			if (!networkManager.IsServer && networkManager.DisconnectReason != string.Empty) {
				Debug.Log($"{clientId} Disconnected: {networkManager.DisconnectReason}");
			}

			//if (networkManager.IsClient) SceneManager.LoadScene("Menu", LoadSceneMode.Single);
		}
	}
}