using PlayerScripts;
using UnityEngine;
using UnityEngine.UI;

namespace Player
{
    public class SkinSelector : MonoBehaviour
    {
        
        private int _selectedSkin;
        private int _selectedFace;
        private int _selectedEars;
        
        [SerializeField] private LobbyPreviewCharacter previewCharacter;

        private void Awake()
        {
            _selectedSkin = PlayerPrefs.GetInt("PlayerSkin", 0);
            _selectedFace = PlayerPrefs.GetInt("PlayerFace", 0);
            _selectedEars = PlayerPrefs.GetInt("PlayerEars", 0);
            previewCharacter.ChangeSkin(_selectedSkin);
            previewCharacter.ChangeFace(_selectedFace);
            previewCharacter.ChangeEars(_selectedEars);
        }

        public void NextSkin()
        {
            if (_selectedSkin < previewCharacter.PlayerSkinsCount - 1) _selectedSkin++;
            else _selectedSkin = 0;
            previewCharacter.ChangeSkin(_selectedSkin);
            PlayerPrefs.SetInt("PlayerSkin",  _selectedSkin);
        }

        public void PreviousSkin()
        {
            if (_selectedSkin > 0) _selectedSkin--;
            else _selectedSkin = previewCharacter.PlayerSkinsCount - 1;
            previewCharacter.ChangeSkin(_selectedSkin);
            PlayerPrefs.SetInt("PlayerSkin",  _selectedSkin);
        }
        
        public void NextFace()
        {
            if (_selectedFace < previewCharacter.PlayerFacesCount - 1) _selectedFace++;
            else _selectedFace = 0;
            previewCharacter.ChangeFace(_selectedFace);
            PlayerPrefs.SetInt("PlayerFace",  _selectedFace);
        }

        public void PreviousFace()
        {
            if (_selectedFace > 0) _selectedFace--;
            else _selectedFace = previewCharacter.PlayerFacesCount - 1;
            previewCharacter.ChangeFace(_selectedFace);
            PlayerPrefs.SetInt("PlayerFace",  _selectedFace);
        }
        public void NextEars()
        {
            if (_selectedEars < previewCharacter.PlayerEarsCount - 1) _selectedEars++;
            else _selectedEars = 0;
            previewCharacter.ChangeEars(_selectedEars);
            PlayerPrefs.SetInt("PlayerEars",  _selectedEars);
        }

        public void PreviousEars()
        {
            if (_selectedEars > 0) _selectedEars--;
            else _selectedEars = previewCharacter.PlayerEarsCount - 1;
            previewCharacter.ChangeEars(_selectedEars);
            PlayerPrefs.SetInt("PlayerEars",  _selectedEars);
        }
    }
}
