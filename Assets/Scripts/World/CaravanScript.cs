using UnityEngine;

namespace World
{
    public class CaravanScript : MonoBehaviour
    {
        private void FixedUpdate()
        {
            transform.Translate(transform.forward * (0.1f * Time.fixedDeltaTime));
        }

        private void OnCollisionEnter(Collision other)
        {
            if(other.transform.parent == null)
            {
                other.transform.SetParent(transform);
            }
        }

        private void OnCollisionExit(Collision other)
        {
            if(other.transform.parent == transform)
            {
                other.transform.SetParent(null);
            }
        }
    }
}
