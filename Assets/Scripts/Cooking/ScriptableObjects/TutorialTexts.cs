using System;
using UnityEngine;

namespace Cooking.ScriptableObjects
{
    
    [CreateAssetMenu(fileName = "TutorialScript", menuName = "Tutorial")]
    public class TutorialTexts : ScriptableObject
    {
        public TutorialText[] texts;
    }

    [Serializable]
    public struct TutorialText
    {
        public string tutorialName;
        [TextArea]
        public string tutorialText;
    }
}