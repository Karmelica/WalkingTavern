using UnityEngine;

namespace Player
{
	public class LobbyPreviewCharacter : MonoBehaviour
	{
		#region Variables

		[SerializeField] private SkinnedMeshRenderer[] playerSkinMesh;
		[SerializeField] private SkinnedMeshRenderer[] playerEarsMesh;
		[SerializeField] private SkinnedMeshRenderer[] playerShirtMesh;
		[SerializeField] private SkinnedMeshRenderer[] playerPantsMesh;
		[SerializeField] private SkinnedMeshRenderer[] playerHairMesh;
		[SerializeField] private SkinnedMeshRenderer playerFaceMesh;

		[SerializeField] private Material[] skinsMaterials;
		[SerializeField] private Material[] facesMaterials;
		[SerializeField] private Material[] clothesMaterials;
		[SerializeField] private Material[] hairMaterials;

		public int PlayerEarsCount => playerEarsMesh.Length;
		public int PlayerSkinsCount => skinsMaterials.Length;
		public int PlayerFacesCount => facesMaterials.Length;
		public int PlayerHairMatCount => hairMaterials.Length;
		public int PlayerClothesMatCount => clothesMaterials.Length;
		public int PlayerPantsCount => playerPantsMesh.Length;
		public int PlayerShirtCount => playerShirtMesh.Length;
		public int PlayerHairCount => playerHairMesh.Length;

		#endregion

		#region Set Customization

		private void Awake()
		{
			var selectedSkin = PlayerPrefs.GetInt("PlayerSkin", 0);
			var selectedFace = PlayerPrefs.GetInt("PlayerFace", 0);
			var selectedEars = PlayerPrefs.GetInt("PlayerEars", 0);
			var selectedPants = PlayerPrefs.GetInt("PlayerPants", 0);
			var selectedShirt = PlayerPrefs.GetInt("PlayerShirt", 0);
			var selectedHair = PlayerPrefs.GetInt("PlayerHair", 0);
			var selectedPantsColor = PlayerPrefs.GetInt("PlayerPantsColor", 0);
			var selectedShirtColor = PlayerPrefs.GetInt("PlayerShirtColor", 0);
			var selectedHairColor = PlayerPrefs.GetInt("PlayerHairColor", 0);
			ChangeSkinColor(selectedSkin);
			ChangeFace(selectedFace);
			ChangeEars(selectedEars);
			ChangePants(selectedPants);
			ChangeShirt(selectedShirt);
			ChangeHair(selectedHair);
			ChangeShirtColor(selectedShirtColor);
			ChangePantsColor(selectedPantsColor);
			ChangeHairColor(selectedHairColor);
		}

		public void ChangeSkinColor(int index)
		{
			foreach (var mesh in playerSkinMesh) mesh.sharedMaterial = skinsMaterials[index];
		}

		public void ChangeShirtColor(int selectedShirtColor)
		{
			foreach (var mesh in playerShirtMesh) mesh.sharedMaterial = clothesMaterials[selectedShirtColor];
		}

		public void ChangePantsColor(int selectedPantsColor)
		{
			foreach (var mesh in playerPantsMesh) mesh.sharedMaterial = clothesMaterials[selectedPantsColor];
		}

		public void ChangeHairColor(int selectedHairColor)
		{
			foreach (var mesh in playerHairMesh) mesh.sharedMaterial = hairMaterials[selectedHairColor];
		}

		public void ChangeFace(int index)
		{
			playerFaceMesh.sharedMaterial = new Material(facesMaterials[index]);
		}

		public void ChangeEars(int index)
		{
			playerEarsMesh.ShowSelectedMesh(index);
		}

		public void ChangeHair(int selectedHair)
		{
			playerHairMesh.ShowSelectedMesh(selectedHair);
		}

		public void ChangeShirt(int selectedShirt)
		{
			playerShirtMesh.ShowSelectedMesh(selectedShirt);
		}

		public void ChangePants(int selectedPants)
		{
			playerPantsMesh.ShowSelectedMesh(selectedPants);
		}

		#endregion
	}
}