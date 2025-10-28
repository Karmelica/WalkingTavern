using System;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace Managers
{
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Events")]
        public EventReference footsteps;
        public EventReference jump;
        public EventReference backgroundMusic;
        
        [Header("Event Instances")]
        private EventInstance _backgroundMusicEvent;
    
        private void OnEnable()
        {
            DependencyResolver.Instance.Register(this);
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            DependencyResolver.Instance.Unregister<AudioManager>();
        }

        /*private void Start()
        {
            _backgroundMusicEvent = InitializeEventInstance(backgroundMusic);
            StartEventInstance(_backgroundMusicEvent);
        }

        private void OnDestroy()
        {
            StopEventInstance(_backgroundMusicEvent);
        }*/

        public void PlayOneShot(EventReference eventReference, Vector3 audioPos = default)
        {
            RuntimeManager.PlayOneShot(eventReference, audioPos);
        }

        #region Event Instances

        private EventInstance InitializeEventInstance(EventReference eventRef, Vector3 audioPos = default)
        {
            var eventInstance = RuntimeManager.CreateInstance(eventRef);
            eventInstance.set3DAttributes(audioPos.To3DAttributes());
            return eventInstance;
        }
        
        private void StartEventInstance(EventInstance eventInstance)
        {
            eventInstance.start();
        }
        
        private void StopEventInstance(EventInstance eventInstance)
        {
            eventInstance.stop(STOP_MODE.ALLOWFADEOUT);
            eventInstance.release();
        }

        #endregion

    }
}
