using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace Managers
{
    public class FMODEvents : MonoBehaviour
    {
        public static FMODEvents Instance { get; private set; }
    
        public EventReference footsteps;
        public EventReference jump;
        public EventReference backgroundMusic;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
