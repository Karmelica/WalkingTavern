using Managers.Network;
using UnityEngine;

public class BootstrapManager : MonoBehaviour
{
    [SerializeField] private GameObject networkManagerPrefab;
    [SerializeField] private GameObject audioManagerPrefab;
    [SerializeField] private GameObject uiManagerPrefab;
    [SerializeField] private GameObject gameManagerPrefab;

    private GameObject _networkManagerInstance;
    private GameObject _audioManagerInstance;
    private GameObject _uiManagerInstance;
    private GameObject _gameManagerInstance;
    
    private void Awake()
    {
        InstantiateManagers();
    }

    private void InstantiateManagers()
    {
        _networkManagerInstance = Instantiate(networkManagerPrefab);
        _audioManagerInstance = Instantiate(audioManagerPrefab);
        _uiManagerInstance = Instantiate(uiManagerPrefab);
        _gameManagerInstance = Instantiate(gameManagerPrefab);
    }
}