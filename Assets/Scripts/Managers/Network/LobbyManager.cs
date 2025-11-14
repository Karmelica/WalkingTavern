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
                ulong lobbyID;
                if (!ulong.TryParse(_clientSteamIdInputField.text, out lobbyID)) return;
                var lobbies = await SteamMatchmaking.LobbyList.WithSlotsAvailable(1).RequestAsync();

                foreach (var lobby in lobbies)
                {
                    if (lobby.Id == lobbyID)
                    {
                        await lobby.Join();
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error joining lobby: " + e.Message);
            }
        }

        public void OnLeaveButtonClicked()
        {
            SteamCurrentLobby.CurrentLobby?.Leave();
            SteamCurrentLobby.CurrentLobby = null;
            NetworkManager.Shutdown();
            SetUI(true);
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