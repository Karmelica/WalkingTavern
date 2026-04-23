using System;
using Managers.Network;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using World.Caravan;

namespace PlayerScripts
{
    public class PlayerGUI : MonoBehaviour
    {
        public static Action<FixedString512Bytes> OnGameInfoChanged;
        
        [SerializeField] private GameObject gameUI;
        [SerializeField] private GameObject pauseUI;
        
        public TextMeshProUGUI interactText;
        public TextMeshProUGUI gameInfoText;


        private void Awake()
        {
            OnGameInfoChanged += UpdateScoreText;
        }

        private void OnDestroy()
        {
            OnGameInfoChanged -= UpdateScoreText;
        }

        private void UpdateScoreText(FixedString512Bytes newInfo)
        {
            gameInfoText.text = newInfo.ToString();
        }

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
