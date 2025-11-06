using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace World
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(NetworkTransform))]
    public class Cube : NetworkBehaviour, IInteractable
    {
        private const float CubeVel = 10f;
        private readonly NetworkVariable<Color> _cubeColor = new (Color.white);
        private Vector3 _interactorPosition;
        private Vector3 _interactorForward;
        private NetworkVariable<bool> _pickedUp = new (false);
        
        private Renderer _cubeRenderer;
        private Rigidbody _rigidbody;
        
        

        private void Update()
        {
            SetCubePositionServerRpc();
        }
        
        private void Awake()
        {
            _cubeRenderer = GetComponent<Renderer>();
            _rigidbody = GetComponent<Rigidbody>();
        }
        
        public void PrimaryInteract(Transform interactor)
        {
            //RandomizeColorServerRpc();
            if (interactor != null && !_pickedUp.Value)
            {
                SetTransformsServerRpc(true, interactor.position, interactor.forward);
            }
            else
            {
                SetTransformsServerRpc(false);
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void SetCubePositionServerRpc()
        {
            _rigidbody.useGravity = !_pickedUp.Value;
            if (!_pickedUp.Value) return;
            _rigidbody.linearVelocity = ((_interactorPosition + _interactorForward * 2f) - transform.position ) * CubeVel;
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void SetTransformsServerRpc(bool isPickedUp, Vector3 transformPosition = default, Vector3 transformForward = default)
        {
            _pickedUp.Value = isPickedUp;
            if (!isPickedUp) return;
            _interactorPosition = transformPosition;
            _interactorForward = transformForward;
        }
        
        // private void SetColor(Color previousValue, Color newValue)
        // {
        //     _cubeRenderer.material.color = _cubeColor.Value;
        // }
        
        // [ServerRpc(RequireOwnership = false)]
        // private void RandomizeColorServerRpc()
        // {
        //     _cubeColor.Value = new Color(Random.value, Random.value, Random.value);
        // }
    }
}

