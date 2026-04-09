using Managers.Network;
using Steamworks;
using TMPro;
using Unity.AppUI.UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using World;

namespace PlayerScripts
{
    public class PlayerGUI : MonoBehaviour
    {
        [SerializeField] private GameObject gameUI;
        [SerializeField] private GameObject pauseUI;
        
        public TextMeshProUGUI interactText;
        
        public void ShowPause(bool show)
        {
            gameUI.SetActive(!show);
            pauseUI.SetActive(show);
        }

        public void LeaveToMenu()
        {
            if(FoodStorage.Instance != null) Destroy(FoodStorage.Instance.gameObject);
            NetworkManager.Singleton.Shutdown();
            SteamCurrentLobby.CurrentLobby?.Leave();
            SteamCurrentLobby.CurrentLobby = null;
            SceneManager.LoadScene("Menu");
        }

        public bool IsPaused()
        {
            return pauseUI.activeSelf;
        }
    }
}
