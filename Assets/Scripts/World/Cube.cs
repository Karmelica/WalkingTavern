using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace World
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(NetworkTransform))]
    public class Cube : NetworkBehaviour, IInteractable
    {
        private readonly NetworkVariable<Color> _cubeColor = new (Color.white);
        private Renderer _cubeRenderer;
        
        public void PrimaryInteract()
        {
            RandomizeColorServerRpc();
        }

        private void Awake()
        {
            _cubeRenderer = GetComponent<Renderer>();
        }
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _cubeColor.OnValueChanged += SetColor;
            SetColor(Color.white, _cubeColor.Value);
        }

        private void SetColor(Color previousValue, Color newValue)
        {
            _cubeRenderer.material.color = _cubeColor.Value;
        }

        [ServerRpc(RequireOwnership = false)]
        private void RandomizeColorServerRpc()
        {
            _cubeColor.Value = new Color(Random.value, Random.value, Random.value);
        }
    }
}

