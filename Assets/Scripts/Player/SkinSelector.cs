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
        private int _selectedPants;
        private int _selectedShirt;
        private int _selectedHair;
        
        [SerializeField] private LobbyPreviewCharacter previewCharacter;

        private void Awake()
        {
            _selectedSkin = PlayerPrefs.GetInt("PlayerSkin", 0);
            _selectedFace = PlayerPrefs.GetInt("PlayerFace", 0);
            _selectedEars = PlayerPrefs.GetInt("PlayerEars", 0);
            _selectedPants = PlayerPrefs.GetInt("PlayerPants", 0);
            _selectedShirt = PlayerPrefs.GetInt("PlayerShirt", 0);
            _selectedHair = PlayerPrefs.GetInt("PlayerHair", 0);
            previewCharacter.ChangeSkin(_selectedSkin);
            previewCharacter.ChangeFace(_selectedFace);
            previewCharacter.ChangeEars(_selectedEars);
            previewCharacter.ChangePants(_selectedPants);
            previewCharacter.ChangeShirt(_selectedShirt);
            previewCharacter.ChangeHair(_selectedHair);
        }

        public void NextSkin()
        {
            _selectedSkin = Next(_selectedSkin, previewCharacter.PlayerSkinsCount);
            previewCharacter.ChangeSkin(_selectedSkin);
            PlayerPrefs.SetInt("PlayerSkin",  _selectedSkin);
        }

        public void PreviousSkin()
        {
            _selectedSkin = Previous(_selectedSkin, previewCharacter.PlayerSkinsCount);
            previewCharacter.ChangeSkin(_selectedSkin);
            PlayerPrefs.SetInt("PlayerSkin",  _selectedSkin);
        }
        
        public void NextFace()
        {
            _selectedFace  = Next(_selectedFace, previewCharacter.PlayerFacesCount);
            previewCharacter.ChangeFace(_selectedFace);
            PlayerPrefs.SetInt("PlayerFace",  _selectedFace);
        }

        public void PreviousFace()
        {
            _selectedFace = Previous(_selectedFace, previewCharacter.PlayerFacesCount);
            previewCharacter.ChangeFace(_selectedFace);
            PlayerPrefs.SetInt("PlayerFace",  _selectedFace);
        }
        
        public void NextEars()
        {
            _selectedEars = Next(_selectedEars, previewCharacter.PlayerEarsCount);
            previewCharacter.ChangeEars(_selectedEars);
            PlayerPrefs.SetInt("PlayerEars",  _selectedEars);
        }

        public void PreviousEars()
        {
            _selectedEars = Previous(_selectedEars, previewCharacter.PlayerEarsCount);
            previewCharacter.ChangeEars(_selectedEars);
            PlayerPrefs.SetInt("PlayerEars",  _selectedEars);
        }

        public void NextShirt()
        {
            _selectedShirt = Next(_selectedShirt, previewCharacter.PlayerShirtCount);
            previewCharacter.ChangeShirt(_selectedShirt);
            PlayerPrefs.SetInt("PlayerShirt",  _selectedShirt);
        }

        public void PreviousShirt()
        {
            _selectedShirt = Previous(_selectedShirt, previewCharacter.PlayerShirtCount);
            previewCharacter.ChangeShirt(_selectedShirt);
            PlayerPrefs.SetInt("PlayerShirt",  _selectedShirt);
        }
        
        public void NextPants()
        {
            _selectedPants = Next(_selectedPants, previewCharacter.PlayerPantsCount);
            previewCharacter.ChangePants(_selectedPants);
            PlayerPrefs.SetInt("PlayerPants",  _selectedPants);
        }

        public void PreviousPants()
        {
            _selectedPants = Previous(_selectedPants, previewCharacter.PlayerPantsCount);
            previewCharacter.ChangePants(_selectedPants);
            PlayerPrefs.SetInt("PlayerPants",  _selectedPants);
        }
        
        public void NextHair()
        {
            _selectedHair = Next(_selectedHair, previewCharacter.PlayerHairCount);
            previewCharacter.ChangeHair(_selectedHair);
            PlayerPrefs.SetInt("PlayerHair",  _selectedHair);
        }

        public void PreviousHair()
        {
            _selectedHair = Previous(_selectedHair, previewCharacter.PlayerHairCount);
            previewCharacter.ChangeHair(_selectedHair);
            PlayerPrefs.SetInt("PlayerHair",  _selectedHair);
        }
        
        private static int Next(int selected, int count)
        {
            if (selected < count - 1) return ++selected;
            return 0;
        }

        private static int Previous(int selected, int count)
        {
            if (selected > 0) return --selected;
            return count - 1;
        }
    }
}
