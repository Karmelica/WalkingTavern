using System;
using Managers;
using Steamworks;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace PlayerScripts
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

        public int PlayerEarsCount => playerEars.Length;
        public int PlayerSkinsCount => skinsMaterials.Length;
        public int PlayerFacesCount => facesMaterials.Length;
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
            ChangeSkin(selectedSkin);
            ChangeFace(selectedFace);
            ChangeEars(selectedEars);
            ChangePants(selectedPants);
            ChangeShirt(selectedShirt);
            ChangeHair(selectedHair);
        }

        public void ChangeSkin(int index)
        {
            foreach (var mesh in playerMesh)
            {
                mesh.materials = new[] { skinsMaterials[index] };
            }
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
