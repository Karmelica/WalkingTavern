using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Managers
{
    public class GameLoader : MonoBehaviour
    {
        [SerializeField] private Slider loadingBar;
        private float _loadingProgress;
        private AsyncOperation _loading;

        private void Start()
        {
            try
            {
                _loading = SceneManager.LoadSceneAsync("Menu");
                _loading.allowSceneActivation = false;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }

        private void Update()
        {
            if (_loading == null) return;
            //1. jeśli (loading < prawdziwy progress) loading++
            //2. jeśli prawdziwy progress >= 0.9 zezwól na aktywacje
            
            if (_loadingProgress < 1f)
            {
                if(_loadingProgress < _loading.progress){
                    _loadingProgress +=  Time.deltaTime;
                }
                
                if (_loadingProgress >= 0.9f)
                {
                    _loading.allowSceneActivation = true;
                }
            }
            loadingBar.value = _loadingProgress / 1f;
        }
    }
}
