using System;
using FMODUnity;
using UnityEngine;

namespace Managers
{
    public class AudioEvents : MonoBehaviour
    {
        public static AudioEvents Instance;
    
        [Header("Audio Events")]
        public EventReference footsteps;
        public EventReference jump;
        public EventReference backgroundMusic;

        private void OnEnable()
        {
            Instance = this;
        }
    }
}
