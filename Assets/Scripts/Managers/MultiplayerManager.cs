using Unity.Netcode;
using UnityEngine;

namespace Managers
{
    public class MultiplayerManager : MonoBehaviour
    {
        public void OnHostButtonClicked()
        {
            NetworkManager.Singleton.StartHost();
        }

        public void OnClientButtonClicked()
        {
            NetworkManager.Singleton.StartClient();
        }

        public void OnServerButtonClicked()
        {
            NetworkManager.Singleton.StartServer();
        }

    }
}