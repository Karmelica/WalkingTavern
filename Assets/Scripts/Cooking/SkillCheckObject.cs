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

        private void Update()
        {
            if(_interactTransform == null && !IsSkillCheckActive()) return;
            worldCanvas.transform.forward = _interactTransform.forward;;
        }

        public void PrimaryInteract(NetworkBehaviourReference interactor, bool pickingUp = true)
        {
        }

        public void SecondaryInteract(NetworkBehaviourReference interactor)
        {
            if (!interactor.TryGet(out Player.Player player)) return;
            _interactTransform = player.GetInteractPoint();
            if(IsSkillCheckActive())
                Debug.Log(skillCheck.TryComplete());
            else
                skillCheck.gameObject.SetActive(true);
        }

        public string GetInteractName()
        {
            return gameObject.name;
        }

        public bool IsPickedUp()
        {
            return false;
        }

        private bool IsSkillCheckActive()
        {
            return skillCheck.gameObject.activeInHierarchy;
        }
    }
}
