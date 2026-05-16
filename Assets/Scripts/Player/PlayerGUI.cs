using System;
using System.Collections.Generic;
using Managers;
using Managers.Network;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using World.Caravan;

namespace PlayerScripts
{
    public class PlayerGUI : MonoBehaviour
    {
        public static Action<FixedString512Bytes> OnGameInfoChanged;
        public static Action<float> OnSnailProgressChanged;
        public static Action<string> OnTutorialTextChanged;
        
        [SerializeField] private GameObject gameUI;
        [SerializeField] private GameObject tutorialUI;
        [SerializeField] private GameObject pauseUI;
        [SerializeField] private Slider snailSlider;
        
        public TextMeshProUGUI interactText;
        public TextMeshProUGUI gameInfoText;
        public TextMeshProUGUI tutorialText;

        private void Awake()
        {
            OnGameInfoChanged += UpdateScoreText;
            OnSnailProgressChanged += SnailSliderChange;
            OnTutorialTextChanged += TutorialTextChanged;
        }

        private void TutorialTextChanged(string newText)
        {
            tutorialUI.SetActive(true);
            tutorialText.text = newText;
            tutorialText.text += "\n\nPress back button to close this window";
        }

        public void CloseTutorialPopup()
        {
            tutorialUI.SetActive(false);
            tutorialText.text = "";
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

        public bool IsTutorialShown()
        {
            return tutorialUI.activeSelf;
        }

        private void OnDestroy()
        {
            OnGameInfoChanged -= UpdateScoreText;
            OnSnailProgressChanged -= SnailSliderChange;
            OnTutorialTextChanged -= TutorialTextChanged;
        }
    }
}
