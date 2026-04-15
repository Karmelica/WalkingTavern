using UnityEngine;

namespace World.Caravan
{
    public class TeleportScript : MonoBehaviour
    {
        public Transform targetTeleportPosition;
        [SerializeField] private TeleportScript target;
        private Teleportable _traveller;

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out _traveller))
            {
                var m = target.targetTeleportPosition.transform.localToWorldMatrix * transform.worldToLocalMatrix * _traveller.transform.localToWorldMatrix;
                _traveller.Teleport(m.GetColumn(3), m.rotation);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if(_traveller)
            {
                _traveller = null;
            }
        }
    }
}


