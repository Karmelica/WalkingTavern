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
        
        public void TrySkillCheck()
        {
            if (!IsSkillCheckActive()) skillCheck.gameObject.SetActive(true);
            else skillCheck.TryComplete();
        }
        
        public bool IsSkillCheckActive()
        {
            return skillCheck.gameObject.activeInHierarchy;
        }
    }
}
