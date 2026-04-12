using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class LevelManager : NetworkBehaviour
    {
        [SerializeField] private SceneAsset nextScene;
        
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            if (IsServer)
            {
                NetworkManager.SceneManager.LoadScene(nextScene.name, LoadSceneMode.Single);
            }
        }
    }
}
