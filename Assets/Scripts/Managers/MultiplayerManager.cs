using System;
using System.Collections.Generic;
using Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class MultiplayerManager : NetworkBehaviour
    {
        //[SerializeField] private string m_SceneName = "SampleScene";
        
        public void OnHostButtonClicked()
        {
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
            NetworkManager.Singleton.StartHost();
            //NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
        }


        public void OnClientButtonClicked()
        {
            NetworkManager.Singleton.StartClient();
            //SceneManager.LoadScene("SampleScene");
        }

        public void OnServerButtonClicked()
        {
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
            NetworkManager.Singleton.StartServer();
            //NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
        }

        /*public override void OnNetworkSpawn()
        {
            if (IsServer && !string.IsNullOrEmpty(m_SceneName))
            {
                var status = NetworkManager.SceneManager.LoadScene(m_SceneName, LoadSceneMode.Single);
                if (status != SceneEventProgressStatus.Started)
                {
                    Debug.LogWarning($"Failed to load {m_SceneName} " +
                                     $"with a {nameof(SceneEventProgressStatus)}: {status}");
                }
            }
        }*/
        
        /*private void OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            if (IsServer)
            {
                NetworkManager.Singleton.SceneManager.UnloadScene(SceneManager.GetSceneByName("Menu"));
            }
        }*/
        
        // private void OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        // {
        //     if (!NetworkManager.Singleton.IsHost || NetworkManager.Singleton.LocalClient.PlayerObject != null) return;
        //     var playerObject = Instantiate(NetworkManager.Singleton.NetworkConfig.PlayerPrefab);
        //     playerObject.transform.position = Vector3.zero + new Vector3(0, 5, 0);
        //     playerObject.GetComponent<PlayerMovement>().playerCamera = Camera.main;
        //     playerObject.GetComponent<NetworkObject>().SpawnAsPlayerObject(NetworkManager.Singleton.LocalClientId);
        // }
        
        private static void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            /*// The client identifier to be authenticated
            var clientId = request.ClientNetworkId;

            // Additional connection data defined by user code
            var connectionData = request.Payload;*/

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

            // Position to spawn the player object (if null it uses default of Vector3.zero)
            response.Position = Vector3.zero + new Vector3(0,5,0);

            // If response.Approved is false, you can provide a message that explains the reason why via ConnectionApprovalResponse.Reason
            // On the client-side, NetworkManager.DisconnectReason will be populated with this message via DisconnectReasonMessage
            //response.Reason = "Some reason for not approving the client";

            // If additional approval steps are needed, set this to true until the additional steps are complete
            // once it transitions from true to false the connection approval response will be processed.
            response.Pending = false;
        }

        /*public override void OnDestroy()
        {
            base.OnDestroy();
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
                //NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
            }
        }*/
    }
}