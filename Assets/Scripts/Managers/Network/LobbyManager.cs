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
            
            // Przypisz callback PRZED uruchomieniem hosta
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
            NetworkManager.Singleton.StartHost();
        }

        private void LobbyEntered(Lobby lobby)
        {
            SteamCurrentLobby.CurrentLobby = lobby;
            lobbyId.text = lobby.Id.ToString();
            ShowPlayers(lobby);
            SetUI(false);

            if(NetworkManager.Singleton.IsHost) return;
            
            var facepunchTransport = NetworkManager.Singleton.GetComponent<FacepunchTransport>();
            facepunchTransport.targetSteamId = lobby.Owner.Id;
            NetworkManager.Singleton.StartClient();
        }


        private async void GameLobbyJoinRequested(Lobby lobby, SteamId steamId)
        {
            try
            {
                await lobby.Join();
            }
            catch (Exception e)
            {
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
            SteamMatchmaking.CreateLobbyAsync(4);
        }

        public async void OnClientButtonClicked()
        {
            try
            {
                if (!ulong.TryParse(_clientSteamIdInputField.text, out ulong lobbyID)) return;
                await SteamMatchmaking.JoinLobbyAsync(lobbyID);

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
            NetworkManager.Singleton.Shutdown();
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        }

        public void OnStartGameButtonClicked()
        {
            if (!NetworkManager.Singleton.IsHost) return;
            NetworkManager.Singleton.SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
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

        #region Connection Approval

        private static void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            if(NetworkManager.Singleton.ConnectedClients.Count >= 4)
            {
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
    }
}