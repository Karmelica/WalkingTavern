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
        public static Action<float> OnSnailProgressChanged;
        
        [SerializeField] private GameObject gameUI;
        [SerializeField] private GameObject pauseUI;
        [SerializeField] private Slider snailSlider;
        
        public TextMeshProUGUI interactText;
        public TextMeshProUGUI gameInfoText;


        private void Awake()
        {
            OnGameInfoChanged += UpdateScoreText;
            OnSnailProgressChanged += SnailSliderChange;
        }


        private void OnDestroy()
        {
            OnGameInfoChanged -= UpdateScoreText;
            OnSnailProgressChanged -= SnailSliderChange;
        }

        private void UpdateScoreText(FixedString512Bytes newInfo)
        {
            gameInfoText.text = newInfo.ToString();
        }
        
        private void SnailSliderChange(float value)
        {
            if (value <= 0 && snailSlider.gameObject.activeInHierarchy) snailSlider.gameObject.SetActive(false);
            else if (value > 0 && !snailSlider.gameObject.activeInHierarchy) snailSlider.gameObject.SetActive(true);
            snailSlider.value = value;
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
