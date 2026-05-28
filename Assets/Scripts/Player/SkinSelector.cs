using UnityEngine;

namespace Player
{
	public class SkinSelector : MonoBehaviour
	{
		[SerializeField] private LobbyPreviewCharacter previewCharacter;
		private int _selectedEars;
		private int _selectedFace;
		private int _selectedHair;
		private int _selectedHairColor;
		private int _selectedPants;
		private int _selectedPantsColor;
		private int _selectedShirt;
		private int _selectedShirtColor;
		private int _selectedSkinColor;

		private void Awake()
		{
			_selectedSkinColor = PlayerPrefs.GetInt("PlayerSkin", 0);
			_selectedFace = PlayerPrefs.GetInt("PlayerFace", 0);
			_selectedEars = PlayerPrefs.GetInt("PlayerEars", 0);
			_selectedPants = PlayerPrefs.GetInt("PlayerPants", 0);
			_selectedShirt = PlayerPrefs.GetInt("PlayerShirt", 0);
			_selectedHair = PlayerPrefs.GetInt("PlayerHair", 0);
			_selectedPantsColor = PlayerPrefs.GetInt("PlayerPantsColor", 0);
			_selectedShirtColor = PlayerPrefs.GetInt("PlayerShirtColor", 0);
			_selectedHairColor = PlayerPrefs.GetInt("PlayerHairColor", 0);
			previewCharacter.ChangeFace(_selectedFace);
			previewCharacter.ChangeSkinColor(_selectedSkinColor);
			previewCharacter.ChangeEars(_selectedEars);
			previewCharacter.ChangePants(_selectedPants);
			previewCharacter.ChangeShirt(_selectedShirt);
			previewCharacter.ChangeHair(_selectedHair);
			previewCharacter.ChangePantsColor(_selectedPantsColor);
			previewCharacter.ChangeShirtColor(_selectedShirtColor);
			previewCharacter.ChangeHairColor(_selectedHairColor);
		}

		public void NextSkin()
		{
			_selectedSkinColor = Next(_selectedSkinColor, previewCharacter.PlayerSkinsCount);
			previewCharacter.ChangeSkinColor(_selectedSkinColor);
			PlayerPrefs.SetInt("PlayerSkin", _selectedSkinColor);
		}

		public void NextShirtColor()
		{
			_selectedShirtColor = Next(_selectedShirtColor, previewCharacter.PlayerClothesMatCount);
			previewCharacter.ChangeShirtColor(_selectedShirtColor);
			PlayerPrefs.SetInt("PlayerShirtColor", _selectedShirtColor);
		}

		public void NextPantsColor()
		{
			_selectedPantsColor = Next(_selectedPantsColor, previewCharacter.PlayerClothesMatCount);
			previewCharacter.ChangePantsColor(_selectedPantsColor);
			PlayerPrefs.SetInt("PlayerPantsColor", _selectedPantsColor);
		}

		public void NextHairColor()
		{
			_selectedHairColor = Next(_selectedHairColor, previewCharacter.PlayerHairMatCount);
			previewCharacter.ChangeHairColor(_selectedHairColor);
			PlayerPrefs.SetInt("PlayerHairColor", _selectedHairColor);
		}

		public void NextFace()
		{
			_selectedFace = Next(_selectedFace, previewCharacter.PlayerFacesCount);
			previewCharacter.ChangeFace(_selectedFace);
			PlayerPrefs.SetInt("PlayerFace", _selectedFace);
		}

		public void PreviousFace()
		{
			_selectedFace = Previous(_selectedFace, previewCharacter.PlayerFacesCount);
			previewCharacter.ChangeFace(_selectedFace);
			PlayerPrefs.SetInt("PlayerFace", _selectedFace);
		}

		public void NextEars()
		{
			_selectedEars = Next(_selectedEars, previewCharacter.PlayerEarsCount);
			previewCharacter.ChangeEars(_selectedEars);
			PlayerPrefs.SetInt("PlayerEars", _selectedEars);
		}

		public void PreviousEars()
		{
			_selectedEars = Previous(_selectedEars, previewCharacter.PlayerEarsCount);
			previewCharacter.ChangeEars(_selectedEars);
			PlayerPrefs.SetInt("PlayerEars", _selectedEars);
		}

		public void NextShirt()
		{
			_selectedShirt = Next(_selectedShirt, previewCharacter.PlayerShirtCount);
			previewCharacter.ChangeShirt(_selectedShirt);
			PlayerPrefs.SetInt("PlayerShirt", _selectedShirt);
		}

		public void PreviousShirt()
		{
			_selectedShirt = Previous(_selectedShirt, previewCharacter.PlayerShirtCount);
			previewCharacter.ChangeShirt(_selectedShirt);
			PlayerPrefs.SetInt("PlayerShirt", _selectedShirt);
		}

		public void NextPants()
		{
			_selectedPants = Next(_selectedPants, previewCharacter.PlayerPantsCount);
			previewCharacter.ChangePants(_selectedPants);
			PlayerPrefs.SetInt("PlayerPants", _selectedPants);
		}

		public void PreviousPants()
		{
			_selectedPants = Previous(_selectedPants, previewCharacter.PlayerPantsCount);
			previewCharacter.ChangePants(_selectedPants);
			PlayerPrefs.SetInt("PlayerPants", _selectedPants);
		}

		public void NextHair()
		{
			_selectedHair = Next(_selectedHair, previewCharacter.PlayerHairCount);
			previewCharacter.ChangeHair(_selectedHair);
			PlayerPrefs.SetInt("PlayerHair", _selectedHair);
		}

		public void PreviousHair()
		{
			_selectedHair = Previous(_selectedHair, previewCharacter.PlayerHairCount);
			previewCharacter.ChangeHair(_selectedHair);
			PlayerPrefs.SetInt("PlayerHair", _selectedHair);
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