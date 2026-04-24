using Managers;
using UnityEngine;

namespace Cooking.Minigames
{
    public class Slicing : IngredientMinigame
    {
        [Range(1, 10)]
        
        private Vector2 _oldMousePos;

        protected override void DoMinigame()
        {
            base.DoMinigame();
            if (!DidHit) return;
            if (RayHit.collider.gameObject)
            {
                // ReSharper disable PossibleLossOfFraction
                if (MousePos.y < Screen.height / 2 && _oldMousePos.y > Screen.height/2)
                {
                    Score++;
                }
            }
            _oldMousePos = MousePos;
        }

        public override string GetInteractText()
        {
            return "Slicing";
        }
    }
}
