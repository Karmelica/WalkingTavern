using Unity.Netcode;
using UnityEngine;

namespace DefaultNamespace
{
    public interface ITeleportable
    {
        public void Teleport(Vector3 position);
    }
}