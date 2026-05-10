using System;
using UnityEngine;

namespace Player
{
    public class LobbyPreviewCharacter : MonoBehaviour
    {
        #region Variables
        
        [SerializeField] private SkinnedMeshRenderer[] playerMesh;
        [SerializeField] private SkinnedMeshRenderer[] playerEars;
        [SerializeField] private SkinnedMeshRenderer[] playerShirt;
        [SerializeField] private SkinnedMeshRenderer[] playerPants;
        [SerializeField] private SkinnedMeshRenderer[] playerHair;
        [SerializeField] private SkinnedMeshRenderer playerFace;
        
        [SerializeField] private Material[] skinsMaterials;
        [SerializeField] private Material[] facesMaterials;
        [SerializeField] private Material[] clothesMaterials;
        [SerializeField] private Material[] hairMaterials;

        public int PlayerEarsCount => playerEars.Length;
        public int PlayerSkinsCount => skinsMaterials.Length;
        public int PlayerFacesCount => facesMaterials.Length;
        public int PlayerHairMatCount => hairMaterials.Length;
        public int PlayerClothesMatCount => clothesMaterials.Length;
        public int PlayerPantsCount => playerPants.Length;
        public int PlayerShirtCount => playerShirt.Length;
        public int PlayerHairCount => playerHair.Length;
        
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
            var selectedPantsColor = PlayerPrefs.GetInt("PlayerHairColor", 0);
            var selectedShirtColor = PlayerPrefs.GetInt("PlayerShirtColor", 0);
            var selectedHairColor = PlayerPrefs.GetInt("PlayerPantsColor", 0);
            ChangeSkinColor(selectedSkin);
            ChangeFace(selectedFace);
            ChangeEars(selectedEars);
            ChangePants(selectedPants);
            ChangeShirt(selectedShirt);
            ChangeHair(selectedHair);
            ChangeShirtColor(selectedShirtColor,selectedShirt);
            ChangePantsColor(selectedPantsColor,selectedPants);
            ChangeHairColor(selectedHairColor,selectedHair);
        }

        public void ChangeSkinColor(int index)
        {
            foreach (var mesh in playerMesh)
            {
                mesh.materials = new[] { skinsMaterials[index] };
            }
        }
        
        public void ChangeShirtColor(int selectedShirtColor, int selectedShirtIndex)
        {
            playerShirt[selectedShirtIndex].sharedMaterial = skinsMaterials[selectedShirtColor];
        }

        public void ChangePantsColor(int selectedPantsColor,  int selectedPantsIndex)
        {
            playerPants[selectedPantsIndex].sharedMaterial = skinsMaterials[selectedPantsColor];
        }
        
        public void ChangeHairColor(int selectedHairColor, int selectedHairIndex)
        {
            playerHair[selectedHairIndex].sharedMaterial = hairMaterials[selectedHairColor];
        }
        
        public void ChangeFace(int index)
        {
            playerFace.materials = new[] { new Material (facesMaterials[index]) };
        }
        
        public void ChangeEars(int index)
        {
            Utilis.ShowSelectedMesh(playerEars, index);
        }

        public void ChangeHair(int selectedHair)
        {
            Utilis.ShowSelectedMesh(playerHair, selectedHair);
        }

        public void ChangeShirt(int selectedShirt)
        {
            Utilis.ShowSelectedMesh(playerShirt, selectedShirt);
        }

        public void ChangePants(int selectedPants)
        {
            Utilis.ShowSelectedMesh(playerPants, selectedPants);
        }

        #endregion

    }
}
