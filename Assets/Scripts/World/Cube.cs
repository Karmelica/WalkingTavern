using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace World
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(NetworkTransform))]
    public class Cube : NetworkBehaviour, IInteractable
    {
        public void PrimaryInteract()
        {
            RandomizeColorServerRpc();
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void RandomizeColorServerRpc()
        {
            var color = new Color(Random.value, Random.value, Random.value);
            RandomizeColorClientRpc(color);
        }
        
        [ClientRpc]
        private void RandomizeColorClientRpc(Color color)
        {
            var cubeRenderer = GetComponent<Renderer>();
            cubeRenderer.material.color = color;
        }
    }
}

