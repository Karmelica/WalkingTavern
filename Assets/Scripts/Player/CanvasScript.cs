using Cooking;
using TMPro;
using UnityEngine;

namespace Player
{
    [DefaultExecutionOrder(-40)]
    public class CanvasScript : MonoBehaviour
    {
        public TextMeshProUGUI interactText;
        public SkillCheck skillCheck;
        
        public void EnableSkillCheck()
        {
            if (!skillCheck.gameObject.activeInHierarchy)
                skillCheck.gameObject.SetActive(true);
            else skillCheck.TryComplete();
        }
    }
}
