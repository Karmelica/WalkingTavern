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
        private float _loadingTime = 1f;
        private float _currentLoadingTime = 0f;
        
        private IEnumerator Start()
        {
            yield return new WaitForSeconds(_loadingTime);
            SceneManager.LoadScene("Menu");
        }

        private void Update()
        {
            _currentLoadingTime += Time.deltaTime;
            loadingBar.value = _currentLoadingTime / _loadingTime;
        }
    }
}
