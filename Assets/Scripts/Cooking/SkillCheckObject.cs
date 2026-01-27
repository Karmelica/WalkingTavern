using System;
using Unity.Netcode;
using UnityEngine;

namespace Cooking
{
    public class SkillCheckObject : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameObject worldCanvas;
        [SerializeField] private SkillCheck skillCheck;
        private Transform _interactTransform;
        private Camera _camera;

        private void Start()
        {
            _camera = Camera.main;
        }

        private void Update()
        {
            if(!_camera) return;
            worldCanvas.transform.forward = _camera.transform.forward;
        }

        public void PrimaryInteract(NetworkBehaviourReference interactor, bool pickingUp = true)
        {
            //nothing to do here
        }

        public void SecondaryInteract(NetworkBehaviourReference interactor)
        {
            if (!interactor.TryGet(out Player.Player player)) return;
            _interactTransform = player.GetInteractPoint();
            skillCheck.AssignPlayer(player);
            if(IsSkillCheckActive())
            {
                skillCheck.TryComplete(player);
            }
            else
            {
                player.SetCanMove(false);
                skillCheck.gameObject.SetActive(true);
            }
                
        }

        public string GetInteractName()
        {
            return gameObject.name;
        }

        public bool IsInteractedWith()
        {
            return false;
        }

        private bool IsSkillCheckActive()
        {
            return skillCheck.gameObject.activeInHierarchy;
        }
    }
}
