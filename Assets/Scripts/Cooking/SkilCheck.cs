using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Cooking
{
    public class SkilCheck : MonoBehaviour
    {
        [SerializeField] private RectTransform skillCheck;
        [SerializeField] private RectTransform skillCheckPointer;
        [SerializeField] private RectTransform skillCheckZone;
        [SerializeField] private TextMeshProUGUI skillCheckText;
        [SerializeField] private float pointerSpeed = 10f;

        private IEnumerator Start()
        {
            while (skillCheckPointer.transform.localPosition.x < skillCheck.sizeDelta.x / 2)
            {
                skillCheckPointer.transform.localPosition += new Vector3(pointerSpeed * Time.deltaTime, 0, 0);
                if(skillCheckPointer.transform.localPosition.x < skillCheckZone.transform.localPosition.x - skillCheckZone.sizeDelta.x / 2)
                {
                    skillCheckText.text = "Before Zone";
                    skillCheckText.color = Color.green;
                }
                if(skillCheckPointer.transform.localPosition.x > skillCheckZone.transform.localPosition.x + skillCheckZone.sizeDelta.x / 2)
                {
                    skillCheckText.text = "After Zone";
                    skillCheckText.color = Color.red;
                }
                if(skillCheckPointer.transform.localPosition.x >= skillCheckZone.transform.localPosition.x - skillCheckZone.sizeDelta.x / 2 &&
                   skillCheckPointer.transform.localPosition.x <= skillCheckZone.transform.localPosition.x + skillCheckZone.sizeDelta.x / 2)
                {
                    skillCheckText.text = "In Zone";
                    skillCheckText.color = Color.yellow;
                }
                yield return null;
            }
        }
    }
}
