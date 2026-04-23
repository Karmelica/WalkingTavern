using System;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class PlayerOptions : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown qualityLevelDropdown;
    [SerializeField] private TMP_Dropdown vSyncDropdown;
    [SerializeField] private Slider framerateSlider;
    [SerializeField] private TextMeshProUGUI framerateText;

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

    private void UpdateOptions()
    {
        framerateSlider.interactable = vSyncDropdown.value == 0;
        framerateSlider.value = Application.targetFrameRate;
        framerateText.text = Mathf.RoundToInt(framerateSlider.value).ToString();
        qualityLevelDropdown.value = QualitySettings.GetQualityLevel();
        vSyncDropdown.value = QualitySettings.vSyncCount;
    }
}
