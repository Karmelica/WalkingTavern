using System;
using FMODUnity;
using UnityEngine;

namespace Managers
{
    public class AudioEvents : MonoBehaviour
    {
        public static AudioEvents Instance;
    
        [Header("Audio Events")]
        [Header("Player")]
        public EventReference footsteps;
        public EventReference jump;
        [Header("Ambient")]
        public EventReference backgroundMusic;
        [Header("Cooking")]
        public EventReference minigameComplete;

        private void OnEnable()
        {
            Instance = this;
        }
    }
}
