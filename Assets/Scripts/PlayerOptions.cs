using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerOptions : MonoBehaviour
{
	[SerializeField] private TMP_Dropdown qualityLevelDropdown;
	[SerializeField] private TMP_Dropdown vSyncDropdown;
	[SerializeField] private Slider framerateSlider;
	[SerializeField] private TextMeshProUGUI framerateText;

	[Header("Volume")] [SerializeField] private Slider masterSlider;

	[SerializeField] private Slider sfxSlider;
	[SerializeField] private Slider ambientSlider;
	[SerializeField] private Slider uiSlider;

	private void Awake()
	{
		UpdateOptions();
	}

	public void ChangeQualityLevel(int index)
	{
		QualitySettings.SetQualityLevel(qualityLevelDropdown.value);

		UpdateOptions();
	}

	public void ChangeVsync(int index)
	{
		QualitySettings.vSyncCount = vSyncDropdown.value;
		UpdateOptions();
	}

	public void ChangeFramerate(int value)
	{
		Application.targetFrameRate = Mathf.RoundToInt(framerateSlider.value);
		UpdateOptions();
	}

	public void ResetTutorials()
	{
		Tutorial.ResetTutorials();
	}

	public void ChangeMasterVolume(float value)
	{
		RuntimeManager.GetVCA("vca:/Master").setVolume(masterSlider.value);

		PlayerPrefs.SetFloat("masterVolume", masterSlider.value);
	}

	public void ChangeAmbientVolume(float value)
	{
		RuntimeManager.GetVCA("vca:/Ambient").setVolume(ambientSlider.value);

		PlayerPrefs.SetFloat("ambientVolume", ambientSlider.value);
	}

	public void ChangeSfxVolume(float value)
	{
		RuntimeManager.GetVCA("vca:/SFX").setVolume(sfxSlider.value);

		PlayerPrefs.SetFloat("sfxVolume", sfxSlider.value);
	}

	public void ChangeUIVolume(float value)
	{
		RuntimeManager.GetVCA("vca:/UI").setVolume(uiSlider.value);

		PlayerPrefs.SetFloat("uiVolume", uiSlider.value);
	}

	private void UpdateOptions()
	{
		if (vSyncDropdown.value == 0) {
			framerateSlider.interactable = true;
			framerateSlider.value = Application.targetFrameRate;
			Application.targetFrameRate = Mathf.RoundToInt(framerateSlider.value);
		} else {
			framerateSlider.interactable = false;
		}

		framerateSlider.value = Application.targetFrameRate;
		framerateText.text = Mathf.RoundToInt(framerateSlider.value).ToString();
		qualityLevelDropdown.value = QualitySettings.GetQualityLevel();
		vSyncDropdown.value = QualitySettings.vSyncCount;

		ambientSlider.value = PlayerPrefs.GetFloat("ambientVolume", 1);
		sfxSlider.value = PlayerPrefs.GetFloat("sfxVolume", 1);
		uiSlider.value = PlayerPrefs.GetFloat("uiVolume", 1);
		masterSlider.value = PlayerPrefs.GetFloat("masterVolume", 1);
	}
}