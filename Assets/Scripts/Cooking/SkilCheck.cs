using System;
using UnityEngine;

namespace Cooking
{
    public class SkilCheck : MonoBehaviour
    {
        [SerializeField] private RectTransform skillCheck;
        [SerializeField] private GameObject SkillCheckPointer;
        [SerializeField] private GameObject SkillCheckZone;
        [SerializeField] private float pointerSpeed = 10f;

        private void Update()
        {
            if (SkillCheckPointer.transform.localPosition.x > skillCheck.sizeDelta.x / 2) return;
            SkillCheckPointer.transform.localPosition += new Vector3(pointerSpeed * Time.deltaTime, 0, 0);
            if(SkillCheckPointer.transform.localPosition.x > SkillCheckZone.transform.localScale.x / 2)
            {
                Debug.Log("SkillCheck Failed");
            }
        }
    }
}
