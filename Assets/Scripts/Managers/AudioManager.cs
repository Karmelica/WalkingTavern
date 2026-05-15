using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace Managers
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;
        
        [Header("Event Instances")]
        private EventInstance _menuMusicEvent;
        private EventInstance _ambientEvent;
        private EventInstance _stirEvent;

        private void OnEnable()
        {
            if(Instance == null) {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }else {
                Destroy(gameObject);
            }
            
            SceneManager.sceneLoaded += SceneLoaded;
        }

        private void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            switch(scene.name)
            {
                case "Menu":
                    StartMenuMusic();
                    break;
                case "GatheringLevel":
                case "MainLevel":
                    StartAmbient();
                    break;
            }
        }

        private void Start()
        {
            _menuMusicEvent = InitializeEventInstance(AudioEvents.Instance.menuMusic);
            _ambientEvent = InitializeEventInstance(AudioEvents.Instance.backgroundMusic);
            _stirEvent = InitializeEventInstance(AudioEvents.Instance.stir);
        }

        private void OnDestroy()
        {
            StopEventInstance(_menuMusicEvent);
            StopEventInstance(_ambientEvent);
            StopEventInstance(_stirEvent);
            if(Instance == this) Instance = null;
        }

        public void PlayOneShot(EventReference eventReference, Vector3 audioPos = default, string groundType = "")
        {
            if(groundType != ""){
                var eventInstance = RuntimeManager.CreateInstance(eventReference);
                eventInstance.set3DAttributes(audioPos.To3DAttributes());
                eventInstance.setParameterByNameWithLabel("GroundType", groundType);
                eventInstance.start();
                eventInstance.release();
            }
            else
                RuntimeManager.PlayOneShot(eventReference, audioPos);
        }

        #region Event Instances

        private EventInstance InitializeEventInstance(EventReference eventRef, Vector3 audioPos = default)
        {
            var eventInstance = RuntimeManager.CreateInstance(eventRef);
            eventInstance.set3DAttributes(audioPos.To3DAttributes());
            return eventInstance;
        }

        public void ButtonSound()
        {
            PlayOneShot(AudioEvents.Instance.buttonClick);
        }

        public void StartMenuMusic()
        {
            StopAmbient();
            _menuMusicEvent.getPlaybackState(out var state);
            if(state == PLAYBACK_STATE.STOPPED)
                StartEventInstance(_menuMusicEvent);
        }
        public void StartAmbient()
        {
            StopMenuMusic();
            _ambientEvent.getPlaybackState(out var state);
            if(state == PLAYBACK_STATE.STOPPED)
                StartEventInstance(_ambientEvent);
        }
        public void StartStirring()
        {
            _stirEvent.getPlaybackState(out var state);
            if(state == PLAYBACK_STATE.STOPPED)
                StartEventInstance(_stirEvent);
            _stirEvent.setPaused(false);
        }
        
        public void StopMenuMusic()
        {
            StopEventInstance(_menuMusicEvent);
        }
        public void StopAmbient()
        {
            StopEventInstance(_ambientEvent);
        }
        public void StopStirring()
        {
            _stirEvent.setPaused(true);
        }
        
        private void StartEventInstance(EventInstance eventInstance)
        {
            eventInstance.start();
        }
        
        private void StopEventInstance(EventInstance eventInstance)
        {
            eventInstance.stop(STOP_MODE.ALLOWFADEOUT);
            //eventInstance.release();
        }

        #endregion

    }
}
