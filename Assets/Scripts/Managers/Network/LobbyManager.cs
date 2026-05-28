using System;
using NaughtyAttributes;
using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using TMPro;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers.Network
{
	public class LobbyManager : MonoBehaviour
	{
		[SerializeField] private GameObject loginUI;
		[SerializeField] private GameObject lobbyUI;
		[SerializeField] private GameObject startGameButton;
		[SerializeField] private GameObject waitingForPlayersText;
		[SerializeField] private TextMeshProUGUI playersInLobby;
		[SerializeField] private TextMeshProUGUI lobbyId;

		[Scene] [SerializeField] private string firstScene;

		[Header("Camera Action")] [SerializeField]
		private Transform menuCamera;

		[SerializeField] private Transform menuLocation;
		[SerializeField] private Transform lobbyLocation;
		private TMP_InputField _clientSteamIdInputField;
		private Transform _targetLocation;

		#region Connection Approval

		private static void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request,
			NetworkManager.ConnectionApprovalResponse response)
		{
			if (NetworkManager.Singleton.ConnectedClients.Count >= 4) {
				response.Approved = false;
				response.Reason = "Server is full";
				response.Pending = false;
				return;
			}

			response.CreatePlayerObject = false;
			response.Approved = true;
			response.Pending = false;
		}

		#endregion

		#region Unity Methods

		private void Awake()
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			_clientSteamIdInputField = GetComponentInChildren<TMP_InputField>();
			_targetLocation = menuLocation;
		}

		private void Update()
		{
			if (menuCamera == null) return;
			menuCamera.position = Vector3.Lerp(menuCamera.position, _targetLocation.position, 0.1f);
			menuCamera.rotation = Quaternion.Lerp(menuCamera.rotation, _targetLocation.rotation, 0.1f);
		}

		private void OnEnable()
		{
			if (!SteamClient.IsValid) return;
			SteamMatchmaking.OnLobbyMemberJoined += PlayerJoined;
			SteamMatchmaking.OnLobbyMemberLeave += PlayerLeft;
			SteamMatchmaking.OnLobbyCreated += LobbyCreated;
			SteamMatchmaking.OnLobbyEntered += LobbyEntered;
			SteamFriends.OnGameLobbyJoinRequested += GameLobbyJoinRequested;
		}

		private void OnDisable()
		{
			if (!SteamClient.IsValid) return;
			SteamMatchmaking.OnLobbyMemberJoined -= PlayerJoined;
			SteamMatchmaking.OnLobbyMemberLeave -= PlayerLeft;
			SteamMatchmaking.OnLobbyCreated -= LobbyCreated;
			SteamMatchmaking.OnLobbyEntered -= LobbyEntered;
			SteamFriends.OnGameLobbyJoinRequested -= GameLobbyJoinRequested;
		}

		#endregion

		#region Lobby Events

		private void LobbyCreated(Result result, Lobby lobby)
		{
			if (result != Result.OK) return;
			lobby.SetPublic();
			lobby.SetJoinable(true);

			NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
			NetworkManager.Singleton.StartHost();
		}

		private void LobbyEntered(Lobby lobby)
		{
			SteamCurrentLobby.CurrentLobby = lobby;

			if (!NetworkManager.Singleton.IsHost) {
				var facepunchTransport = NetworkManager.Singleton.GetComponent<FacepunchTransport>();
				facepunchTransport.targetSteamId = lobby.Owner.Id;
				var startClient = NetworkManager.Singleton.StartClient();
				if (!startClient) return;
			}

			lobbyId.text = lobby.Id.ToString();
			ShowPlayers(lobby);
			SetUI(false);
			_targetLocation = lobbyLocation;
		}
		
		private void LobbyEntered()
		{
			NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
			NetworkManager.Singleton.StartHost();
			
			_targetLocation = lobbyLocation;
			SetUI(true);
		}

		private async void GameLobbyJoinRequested(Lobby lobby, SteamId steamId)
		{
			try {
				await lobby.Join();
			} catch (Exception e) {
				Debug.LogError($"Error joining lobby from invite: {e.Message}\n{e.StackTrace}");
			}
		}

		private void PlayerJoined(Lobby lobby, Friend friend)
		{
			ShowPlayers(lobby);
		}

		private void PlayerLeft(Lobby lobby, Friend friend)
		{
			ShowPlayers(lobby);
		}

		#endregion

		#region Buttons

		public void OnHostButtonClicked()
		{
			if (SteamClient.IsValid) {
				SteamMatchmaking.CreateLobbyAsync(4);
			} else {
				LobbyEntered();
			}
		}

		public async void OnClientButtonClicked()
		{
			try {
				if (!SteamClient.IsValid) return;
				if (!ulong.TryParse(_clientSteamIdInputField.text, out var lobbyID)) return;
				await SteamMatchmaking.JoinLobbyAsync(lobbyID);
			} catch (Exception e) {
				Debug.LogError($"Error joining lobby: {e.Message}\n{e.StackTrace}");
			}
		}

		public void OnExitButtonClicked()
		{
			Application.Quit();
#if UNITY_EDITOR
			EditorApplication.isPlaying = false;
#endif
		}

		public void OnLeaveButtonClicked()
		{
			if (!SteamClient.IsValid) {
				NetworkManager.Singleton.Shutdown();
				NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
				_targetLocation = menuLocation;
				SetUI(true);
				
			} else {
				SteamCurrentLobby.CurrentLobby?.Leave();
				SteamCurrentLobby.CurrentLobby = null;
				SetUI(false);
				NetworkManager.Singleton.Shutdown();
				NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
				_targetLocation = menuLocation;
				
			}
		}

		public void OnStartGameButtonClicked()
		{
			if (!NetworkManager.Singleton.IsHost) return;
			SteamCurrentLobby.CurrentLobby?.SetJoinable(false);
			NetworkManager.Singleton.SceneManager.LoadScene(firstScene, LoadSceneMode.Single);
			NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
		}

		#endregion

		#region UI Changes

		public void CopyID()
		{
			var textEditor = new TextEditor
			{
				text = lobbyId.text
			};
			textEditor.SelectAll();
			textEditor.Copy();
		}

		private void SetUI(bool offline)
		{
			if (offline) {
				loginUI.SetActive(_targetLocation == menuLocation);
				lobbyUI.SetActive(_targetLocation != menuLocation);
			} else {
				loginUI.SetActive(SteamCurrentLobby.CurrentLobby == null);
				lobbyUI.SetActive(SteamCurrentLobby.CurrentLobby != null);
			}

			startGameButton.SetActive(NetworkManager.Singleton.IsHost);
			waitingForPlayersText.SetActive(!NetworkManager.Singleton.IsHost);
		}

		private void ShowPlayers(Lobby lobby)
		{
			playersInLobby.text = "Players in Lobby:\n";
			foreach (var player in lobby.Members) playersInLobby.text += player.Name + "\n";
		}

		#endregion
	}
}