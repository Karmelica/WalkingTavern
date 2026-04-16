using UnityEngine;
using UnityEngine.UI;

namespace Player
{
    public class SkinSelector : MonoBehaviour
    {
        [SerializeField] private Material[] playerSkinMaterial;
        [SerializeField] private Material[] playerFaceMaterial;
        
        [SerializeField] private RawImage skinPreviewImage;
        [SerializeField] private RawImage facePreviewImage;
        
        private int _selectedSkin;
        private int _selectedFace;

        private void OnEnable()
        {
            _selectedSkin = PlayerPrefs.GetInt("PlayerSkin", 0);
            _selectedFace = PlayerPrefs.GetInt("PlayerFace", 0);
            skinPreviewImage.material = playerSkinMaterial[_selectedSkin];
            facePreviewImage.material = playerFaceMaterial[_selectedFace];
        }

        public void NextSkin()
        {
            if (_selectedSkin < playerSkinMaterial.Length - 1) _selectedSkin++;
            else _selectedSkin = 0;
            skinPreviewImage.material = playerSkinMaterial[_selectedSkin];
            PlayerPrefs.SetInt("PlayerSkin",  _selectedSkin);
        }

        public void PreviousSkin()
        {
            if (_selectedSkin > 0) _selectedSkin--;
            else _selectedSkin = playerSkinMaterial.Length - 1;
            skinPreviewImage.material = playerSkinMaterial[_selectedSkin];
            PlayerPrefs.SetInt("PlayerSkin",  _selectedSkin);
        }
        
        public void NextFace()
        {
            if (_selectedFace < playerFaceMaterial.Length - 1) _selectedFace++;
            else _selectedFace = 0;
            facePreviewImage.material = playerFaceMaterial[_selectedFace];
            PlayerPrefs.SetInt("PlayerFace",  _selectedFace);
        }

        public void PreviousFace()
        {
            if (_selectedFace > 0) _selectedFace--;
            else _selectedFace = playerFaceMaterial.Length - 1;
            facePreviewImage.material = playerFaceMaterial[_selectedFace];
            PlayerPrefs.SetInt("PlayerFace",  _selectedFace);
        }
    }
}
