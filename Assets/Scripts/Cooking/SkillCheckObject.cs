using System;
using Unity.Netcode;
using UnityEngine;

namespace Cooking
{
    public class SkillCheckObject : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameObject worldCanvas;
        [SerializeField] private SkillCheck skillCheck;
        private bool _isEnabled;

        private void OnEnable()
        {
            _isEnabled = true;
        }

        private void OnDisable()
        {
            _isEnabled = false;
        }

        public void PrimaryInteract(NetworkBehaviourReference interactor, bool pickingUp = true)
        {
            //nothing to do here
        }

        public void SecondaryInteract(NetworkBehaviourReference interactor)
        {
            if (!_isEnabled) return;
            if(IsSkillCheckActive())
            {
                skillCheck.TryComplete();
            }
            else
            {
                skillCheck.gameObject.SetActive(true);
            }
        }

        public string GetInteractName()
        {
            return gameObject.name;
        }
 
        public bool IsInteractedWith()
        {
            return _isEnabled;
        }

        private bool IsSkillCheckActive()
        {
            return skillCheck.gameObject.activeInHierarchy;
        }
    }
}
