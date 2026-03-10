using System;
using UnityEngine;
using UnityEngine.UI;

namespace PlayerScripts
{
    public class SkinSelector : MonoBehaviour
    {
        [SerializeField] private Material[] playerSkinMaterial;
        [SerializeField] private RawImage previewImage;
        private int _selectedSkin;

        private void OnEnable()
        {
            _selectedSkin = PlayerPrefs.GetInt("PlayerSkin", 0);
            previewImage.material = playerSkinMaterial[_selectedSkin];
        }

        public void NextSkin()
        {
            if (_selectedSkin < playerSkinMaterial.Length - 1) _selectedSkin++;
            else _selectedSkin = 0;
            previewImage.material = playerSkinMaterial[_selectedSkin];
            PlayerPrefs.SetInt("PlayerSkin",  _selectedSkin);
        }

        public void PreviousSkin()
        {
            if (_selectedSkin > 0) _selectedSkin--;
            else _selectedSkin = playerSkinMaterial.Length - 1;
            previewImage.material = playerSkinMaterial[_selectedSkin];
            PlayerPrefs.SetInt("PlayerSkin",  _selectedSkin);
        }
    }
}
