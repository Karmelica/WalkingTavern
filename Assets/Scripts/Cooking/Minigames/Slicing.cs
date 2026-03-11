using UnityEngine;

namespace Cooking.Minigames
{
    public class Slicing : Minigame
    {
        [Range(1, 10)]
        [SerializeField] private float requiredSpeed = 4f;
        
        private Vector2 _oldMousePos;

        protected override void DoMinigame(RaycastHit hit, Vector3 mousePos)
        {
            if (hit.collider.gameObject && hit.collider.gameObject == CurrentFood.gameObject)
            {
                var difference = (_oldMousePos.y - mousePos.y) / Screen.height * Time.deltaTime * 1000f;
                if (mousePos.y < _oldMousePos.y && difference > requiredSpeed)
                {
                    CurrentFood.GetComponent<MeshRenderer>().material.color = Random.ColorHSV();
                    Score++;
                }
            }
            _oldMousePos = mousePos;
        }
    }
}
