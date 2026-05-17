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
        public EventReference menuMusic;
        public EventReference fireplace;
        [Header("Cooking")]
        public EventReference minigameComplete;
        public EventReference slice;
        public EventReference stir;
        [Header("Customer")]
        public EventReference money;
        [Header("UI")]
        public EventReference buttonClick;
        [Header("Objects")]
        public EventReference itemDrop;
        public EventReference itemPickup;
        

        private void OnEnable()
        {
            Instance = this;
        }
    }
}
