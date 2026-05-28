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
		private EventInstance _ambientEvent;
		private EventInstance _fireplaceEvent;
		private EventInstance _footstepsEvent;

		[Header("Event Instances")] private EventInstance _menuMusicEvent;

		private EventInstance _stirEvent;

		private void Start()
		{
			_menuMusicEvent = InitializeEventInstance(AudioEvents.Instance.menuMusic);
			_fireplaceEvent = InitializeEventInstance(AudioEvents.Instance.fireplace);
			_ambientEvent = InitializeEventInstance(AudioEvents.Instance.backgroundMusic);
			_stirEvent = InitializeEventInstance(AudioEvents.Instance.stir);
			_footstepsEvent = InitializeEventInstance(AudioEvents.Instance.footsteps);
		}

		private void OnEnable()
		{
			if (Instance == null) {
				Instance = this;
				DontDestroyOnLoad(gameObject);
			} else {
				Destroy(gameObject);
			}

			SceneManager.sceneLoaded += SceneLoaded;
		}

		private void OnDestroy()
		{
			ReleaseEventInstance(_menuMusicEvent);
			ReleaseEventInstance(_fireplaceEvent);
			ReleaseEventInstance(_ambientEvent);
			ReleaseEventInstance(_stirEvent);
			ReleaseEventInstance(_footstepsEvent);
			if (Instance == this) Instance = null;
		}

		private void SceneLoaded(Scene scene, LoadSceneMode mode)
		{
			switch (scene.name) {
				case "Menu":
					StartMenuMusic();
					break;
				case "GatheringLevel":
				case "MainLevel":
					StartAmbient();
					break;
			}
		}

		public void PlayOneShot(EventReference eventReference, Vector3 audioPos = default)
		{
			RuntimeManager.PlayOneShot(eventReference, audioPos);
		}

		public void PlayFootSteps(Vector3 audioPos, string groundType)
		{
			_footstepsEvent.set3DAttributes(audioPos.To3DAttributes());
			_footstepsEvent.setParameterByNameWithLabel("GroundType", groundType);
			_footstepsEvent.start();
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

		public void StartFireplace(Vector3 audioPos)
		{
			_fireplaceEvent.set3DAttributes(audioPos.To3DAttributes());
			_fireplaceEvent.start();
		}

		public void StopFireplace()
		{
			StopEventInstance(_fireplaceEvent);
		}

		private void StartMenuMusic()
		{
			StopAmbient();
			_menuMusicEvent.getPlaybackState(out var state);
			if (!_menuMusicEvent.isValid() || state == PLAYBACK_STATE.STOPPED) {
				StartEventInstance(_menuMusicEvent);
			}
		}

		private void StartAmbient()
		{
			StopMenuMusic();
			_ambientEvent.getPlaybackState(out var state);
			if (!_ambientEvent.isValid() || state == PLAYBACK_STATE.STOPPED) {
				StartEventInstance(_ambientEvent);
			}
		}

		public void StartStirring()
		{
			_stirEvent.getPlaybackState(out var state);
			if (state == PLAYBACK_STATE.STOPPED) {
				StartEventInstance(_stirEvent);
			}

			_stirEvent.setPaused(false);
		}

		private void StopMenuMusic()
		{
			StopEventInstance(_menuMusicEvent);
		}

		private void StopAmbient()
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
		}

		private void ReleaseEventInstance(EventInstance eventInstance)
		{
			eventInstance.stop(STOP_MODE.ALLOWFADEOUT);
			eventInstance.release();
		}

		#endregion
	}
}