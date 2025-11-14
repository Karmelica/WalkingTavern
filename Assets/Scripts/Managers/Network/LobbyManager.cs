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

        private void PlayerJoined(Lobby lobby, Friend friend)
        {
            Debug.Log($"{friend.Name} joined the lobby.");
            ShowPlayers(lobby);
        }


        private void PlayerLeft(Lobby lobby, Friend friend)
        {
            Debug.Log($"{friend.Name} left the lobby.");
            ShowPlayers(lobby);
        }
        
        private void ShowPlayers(Lobby lobby)
        {
            playersInLobby.text = "Players in Lobby:\n";
            foreach (var player in lobby.Members)
            {
                playersInLobby.text += player.Name + "\n";
            }
        }

        private void LobbyCreated(Result result, Lobby lobby)
        {
            if (result != Result.OK) return;
            lobby.SetPublic();
            lobby.SetJoinable(true);
            //Debug.Log("Lobby created with ID: " + lobby.Id);
        }

        private void LobbyEntered(Lobby lobby)
        {
            SteamCurrentLobby.CurrentLobby = lobby;
            lobbyId.text = lobby.Id.ToString();
            //Debug.Log("Joined lobby with ID: " + lobby.Id);
            SetUI(false);
        }

        private void SetUI(bool login)
        {
            loginUI.SetActive(login);
            lobbyUI.SetActive(!login);
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

        public void OnHostButtonClicked()
        {
            SteamMatchmaking.CreateLobbyAsync(4);
        }

        public async void OnClientButtonClicked()
        {
            try
            {
                if (!ulong.TryParse(_clientSteamIdInputField.text, out var lobbyID)) return;
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
            SetUI(true);
        }

        public void CopyID()
        {
            var textEditor = new TextEditor();
            textEditor.text = lobbyId.text;
            textEditor.SelectAll();
            textEditor.Copy();
        }

        /*public void OnHostButtonClicked()
        {
            loginUI.SetActive(false);
            NetworkManager.ConnectionApprovalCallback += ApprovalCheck;
            NetworkManager.StartHost();
            if (!NetworkManager.IsServer) return;
            LoadGame();
        }

        private static void LoadGame()
        {
            NetworkManager.SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
            //Debug.Log("Host started with SteamID: " + SteamClient.SteamId);
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
        }*/

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
            //response.CreatePlayerObject = true;
            response.CreatePlayerObject = false;

            //response.PlayerPrefabHash = 1959477017;

            //response.Position = Vector3.zero + new Vector3(0, 1, 0);
            response.Pending = false;
        }
    }
}