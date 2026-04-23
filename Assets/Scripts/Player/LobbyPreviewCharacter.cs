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
        [SerializeField] private SkinnedMeshRenderer playerFace;
        
        [SerializeField] private Material[] skinsMaterials;
        [SerializeField] private Material[] facesMaterials;

        public int PlayerEarsCount => playerEars.Length;
        public int PlayerSkinsCount => skinsMaterials.Length;
        public int PlayerFacesCount => facesMaterials.Length;
        
        #endregion

        #region Set Customization

        private void Awake()
        {
            
            var selectedSkin = PlayerPrefs.GetInt("PlayerSkin", 0);
            var selectedFace = PlayerPrefs.GetInt("PlayerFace", 0);
            var selectedEars = PlayerPrefs.GetInt("PlayerEars", 0);
            ChangeSkin(selectedSkin);
            ChangeFace(selectedFace);
            ChangeEars(selectedEars);
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
            for(var i = 0; i < playerEars.Length; i++)
            {
                playerEars[i].enabled = i == index;
            }
        }

        #endregion

    }
}
