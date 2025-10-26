using System;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace Managers
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }
        
        private EventInstance _backgroundMusicEvent;
    
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
        
        private void Start()
        {
            //InitializeEventInstance(out _backgroundMusicEvent, transform);
            //StartEventInstance(_backgroundMusicEvent);
        }

        private void OnDestroy()
        {
            //StopEventInstance(_backgroundMusicEvent);
        }

        public void PlayOneShot(EventReference eventReference, Transform position)
        {
            RuntimeManager.PlayOneShot(eventReference, position.position);
        }

        #region Event Instances

        private void InitializeEventInstance(out EventInstance eventInstance, Transform transform)
        {
            eventInstance = RuntimeManager.CreateInstance(FMODEvents.Instance.backgroundMusic);
            eventInstance.set3DAttributes(transform.To3DAttributes());
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
