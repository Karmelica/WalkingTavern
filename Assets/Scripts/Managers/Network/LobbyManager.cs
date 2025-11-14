using System;
using System.Threading.Tasks;
using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers.Network
{
    public class LobbyManager : MonoBehaviour
    {
        [SerializeField] private GameObject loginUI;
        [SerializeField] private GameObject lobbyUI;
        [SerializeField] private TextMeshProUGUI playersInLobby;
        [SerializeField] private TextMeshProUGUI lobbyId;
        private static NetworkManager NetworkManager => NetworkManager.Singleton;
        
        private TMP_InputField _clientSteamIdInputField;

        #region Unity Methods

        void Awake()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            _clientSteamIdInputField = GetComponentInChildren<TMP_InputField>();
        }

        private void OnEnable()
        {
            SteamMatchmaking.OnLobbyMemberJoined += PlayerJoined;
            SteamMatchmaking.OnLobbyMemberLeave += PlayerLeft;
            SteamMatchmaking.OnLobbyCreated += LobbyCreated;
            SteamMatchmaking.OnLobbyEntered += LobbyEntered;
            SteamFriends.OnGameLobbyJoinRequested += GameLobbyJoinRequested;
        }

        private void OnDisable()
        {
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
            NetworkManager.StartHost();
            NetworkManager.ConnectionApprovalCallback += ApprovalCheck;
        }

        private void LobbyEntered(Lobby lobby)
        {
            SteamCurrentLobby.CurrentLobby = lobby;
            lobbyId.text = lobby.Id.ToString();
            ShowPlayers(lobby);
            SetUI(false);

            if(NetworkManager.IsHost) return;
            var facepunchTransport = NetworkManager.Singleton.GetComponent<FacepunchTransport>();
            facepunchTransport.targetSteamId = lobby.Owner.Id;
            
            NetworkManager.StartClient();
        }
        
        private async void GameLobbyJoinRequested(Lobby lobby, SteamId steamId)
        {
            try
            {
                await lobby.Join();
            }
            catch (Exception e)
            {
                Debug.LogError("Error joining lobby from invite: " + e.Message);
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
            SteamMatchmaking.CreateLobbyAsync(4);
        }

        public async void OnClientButtonClicked()
        {
            try
            {
                if (!ulong.TryParse(_clientSteamIdInputField.text, out ulong lobbyID))
                {
                    return;
                }

                // Bezpośrednie dołączenie do lobby po ID
                var lobby = await SteamMatchmaking.JoinLobbyAsync(lobbyID);

                if (!lobby.HasValue)
                {
                    Debug.LogError("Failed to join lobby with ID: " + lobbyID);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error joining lobby: {e.Message}\n{e.StackTrace}");
            }
        }


        public void OnLeaveButtonClicked()
        {
            SteamCurrentLobby.CurrentLobby?.Leave();
            SteamCurrentLobby.CurrentLobby = null;
            SetUI(true);
            if(NetworkManager.IsHost) NetworkManager.ConnectionApprovalCallback -= ApprovalCheck;
            NetworkManager.Shutdown();
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
        
        private void SetUI(bool login)
        {
            loginUI.SetActive(login);
            lobbyUI.SetActive(!login);
        }
        
        private void ShowPlayers(Lobby lobby)
        {
            playersInLobby.text = "Players in Lobby:\n";
            foreach (var player in lobby.Members)
            {
                playersInLobby.text += player.Name + "\n";
            }
        }

        #endregion

        private static void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            if(NetworkManager.Singleton.ConnectedClients.Count >= 4)
            {
                response.Approved = false;
                response.Reason = "Server is full";
                response.Pending = false;
                return;
            }
            
            //response.CreatePlayerObject = true;
            //response.PlayerPrefabHash = 1959477017;
            //response.Position = Vector3.zero + new Vector3(0, 1, 0);
            response.CreatePlayerObject = false;
            response.Approved = true;
            response.Pending = false;
        }
    }
}